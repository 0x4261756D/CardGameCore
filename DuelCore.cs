using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CardGameUtils;
using static CardGameUtils.Functions;
using static CardGameUtils.Structs.NetworkingStructs;

namespace CardGameCore;

class DuelCore : Core
{
	private static GameConstants.State _state = GameConstants.State.UNINITIALIZED;
	public static GameConstants.State State
	{
		get
		{
			return _state;
		}
		set
		{
			Log($"STATE: {_state} -> {value}");
			_state = value;
		}
	}
	public static int UIDCount;
	public Player[] players;
	public static NetworkStream?[] playerStreams = [];
	public static Random rnd = new(Program.seed);
	public const int HASH_LEN = 96;
	private const string AbilityUseActionDescription = "Use";
	private const string CastActionDescription = "Cast";
	private const string CreatureMoveActionDescription = "Move";
	public int playersConnected = 0;
	public int turn, turnPlayer, initPlayer, nextMomentumIncreaseIndex;
	public int? markedZone = null;
	public int momentumBase = GameConstants.START_MOMENTUM;
	public bool rewardClaimed = false;

	private readonly Dictionary<int, List<Trigger>> castTriggers = [];
	private readonly Dictionary<int, List<LocationBasedTargetingTrigger>> genericCastTriggers = [];
	private readonly Dictionary<int, List<TokenCreationTrigger>> tokenCreationTriggers = [];
	private readonly Dictionary<int, List<LocationBasedTargetingTrigger>> genericEnterFieldTriggers = [];
	private readonly Dictionary<int, List<Trigger>> revelationTriggers = [];
	private readonly Dictionary<int, List<Trigger>> victoriousTriggers = [];
	private readonly Dictionary<int, List<Trigger>> attackTriggers = [];
	private readonly Dictionary<int, List<CreatureTargetingTrigger>> deathTriggers = [];
	private readonly Dictionary<int, List<CreatureTargetingTrigger>> genericDeathTriggers = [];
	private readonly Dictionary<int, List<LocationBasedTrigger>> youDiscardTriggers = [];
	private readonly Dictionary<int, List<Trigger>> discardTriggers = [];
	private readonly Dictionary<int, List<StateReachedTrigger>> stateReachedTriggers = [];
	private readonly List<StateReachedTrigger> alwaysActiveStateReachedTriggers = [];
	private readonly Dictionary<int, LingeringEffectList> lingeringEffects = [];
	private readonly Dictionary<int, LingeringEffectList> temporaryLingeringEffects = [];
	private readonly LingeringEffectList alwaysActiveLingeringEffects;
	private readonly Dictionary<int, List<ActivatedEffectInfo>> activatedEffects = [];
	private readonly Dictionary<int, List<Trigger>> dealsDamageTriggers = [];

	private class LingeringEffectList : IEnumerable<LingeringEffectInfo>
	{
		private readonly List<LingeringEffectInfo> items = [];
		private readonly DuelCore core;
		public LingeringEffectList(DuelCore core)
		{
			this.core = core;
			core.EvaluateLingeringEffects();
		}

		public void Add(LingeringEffectInfo info)
		{
			items.Add(info);
			Log($"Added lingering effect, now evaluating lingering effects...");
			core.EvaluateLingeringEffects();
		}

		public void RemoveAll(Predicate<LingeringEffectInfo> condition)
		{
			if(items.RemoveAll(condition) > 0)
			{
				core.EvaluateLingeringEffects();
			}
		}

		public IEnumerator<LingeringEffectInfo> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}
	}

	public DuelCore()
	{
		RegisterScriptingFunctions();
		alwaysActiveLingeringEffects = new(this);
		players = new Player[Program.config.duel_config!.players.Length];
		playerStreams = new NetworkStream[Program.config.duel_config.players.Length];
		for(int i = 0; i < players.Length; i++)
		{
			Log("Player created. ID: " + Program.config.duel_config.players[i].id);
			Deck deck = new();
			GameConstants.PlayerClass playerClass = Enum.Parse<GameConstants.PlayerClass>(Program.config.duel_config.players[i].decklist[0]);
			string abilityString = Program.config.duel_config.players[i].decklist[1];
			if(!abilityString.StartsWith('#'))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no ability, {abilityString} is no suitable ability");
				return;
			}
			Card ability = CreateBasicCard(Type.GetType(CardNameToFilename(abilityString[1..]))!, i);
			string questString = Program.config.duel_config.players[i].decklist[2];
			if(!questString.StartsWith('|'))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no quest, {questString} is no suitable ability");
				return;
			}
			Quest quest = (Quest)CreateBasicCard(Type.GetType(CardNameToFilename(questString[1..]))!, i);
			foreach(string cardString in Program.config.duel_config.players[i].decklist[3..])
			{
				Log($"Creating {cardString}");
				deck.Add(CreateBasicCard(Type.GetType(CardNameToFilename(cardString))!, i));
			}
			players[i] = new Player(Program.config.duel_config.players[i], i, deck, playerClass, ability, quest);
		}
	}

	public void RegisterScriptingFunctions()
	{
		Card.RegisterCastTrigger = RegisterCastTriggerImpl;
		Card.RegisterGenericCastTrigger = RegisterGenericCastTriggerImpl;
		Card.RegisterGenericEntersFieldTrigger = RegisterGenericEntersFieldTriggerImpl;
		Card.RegisterRevelationTrigger = RegisterRevelationTriggerImpl;
		Card.RegisterYouDiscardTrigger = RegisterYouDiscardTriggerImpl;
		Card.RegisterDiscardTrigger = RegisterDiscardTriggerImpl;
		Card.RegisterStateReachedTrigger = RegisterStateReachedTriggerImpl;
		Card.RegisterVictoriousTrigger = RegisterVictoriousTriggerImpl;
		Card.RegisterAttackTrigger = RegisterAttackTriggerImpl;
		Card.RegisterDeathTrigger = RegisterDeathTriggerImpl;
		Card.RegisterGenericDeathTrigger = RegisterGenericDeathTriggerImpl;
		Card.RegisterLingeringEffect = RegisterLingeringEffectImpl;
		Card.RegisterTemporaryLingeringEffect = RegisterTemporaryLingeringEffectImpl;
		Card.RegisterActivatedEffect = RegisterActivatedEffectImpl;
		Card.RegisterTokenCreationTrigger = RegisterTokenCreationTriggerImpl;
		Card.GetGrave = GetGraveImpl;
		Card.GetField = GetFieldImpl;
		Card.GetFieldUsed = GetFieldUsedImpl;
		Card.GetHand = GetHandImpl;
		Card.SelectCards = SelectCardsImpl;
		Card.Discard = DiscardImpl;
		Card.DiscardAmount = DiscardAmountImpl;
		Card.CreateTokenOnField = CreateTokenOnFieldImpl;
		Card.CreateToken = CreateTokenImpl;
		Card.CreateTokenCopyOnField = CreateTokenCopyOnFieldImpl;
		Card.CreateTokenCopy = CreateTokenCopyImpl;
		Card.GetDiscardCountXTurnsAgo = GetDiscardCountXTurnsAgoImpl;
		Card.GetDamageDealtXTurnsAgo = GetDamageDealtXTurnsAgoImpl;
		Card.GetSpellDamageDealtXTurnsAgo = GetSpellDamageDealtXTurnsAgoImpl;
		Card.GetBrittleDeathCountXTurnsAgo = GetBrittleDeathCountXTurnsAgoImpl;
		Card.GetDeathCountXTurnsAgo = GetDeathCountXTurnsAgoImpl;
		Card.PlayerChangeLife = PlayerChangeLifeImpl;
		Card.PlayerChangeMomentum = PlayerChangeMomentumImpl;
		Card.Cast = CastImpl;
		Card.Draw = DrawImpl;
		Card.Destroy = DestroyImpl;
		Card.AskYesNo = AskYesNoImpl;
		Card.GetIgniteDamage = GetIgniteDamageImpl;
		Card.ChangeIgniteDamage = ChangeIgniteDamageImpl;
		Card.ChangeIgniteDamageTemporary = ChangeIgniteDamageTemporaryImpl;
		Card.GetTurn = GetTurnImpl;
		Card.GetPlayerLife = GetPlayerLifeImpl;
		Card.PayLife = PayLifeImpl;
		Card.Gather = GatherImpl;
		Card.Move = MoveImpl;
		Card.SelectZone = SelectZoneImpl;
		Card.MoveToHand = MoveToHandImpl;
		Card.MoveToField = MoveToFieldImpl;
		Card.GetCastCount = GetCastCountImpl;
		Card.ReturnCardsToDeck = ReturnCardsToDeckImpl;
		Card.Reveal = RevealImpl;
		Card.GetDiscardable = GetDiscardableImpl;
		Card.RefreshAbility = ResetAbilityImpl;
		Card.RegisterDealsDamageTrigger = RegisterDealsDamageTriggerImpl;
		Card.CreatureChangeLife = CreatureChangeLifeImpl;
		Card.CreatureChangePower = CreatureChangePowerImpl;
	}

	public override void Init(PipeStream? pipeStream)
	{
		listener.Start();
		pipeStream?.WriteByte(42);
		pipeStream?.Close();
		Log("Listening", severity: LogSeverity.Warning);
		HandleNetworking();
		foreach(NetworkStream? stream in playerStreams)
		{
			stream?.Dispose();
		}
		listener.Stop();
	}

	private static Card CreateBasicCard(Type type, int controller)
	{
		Card card = (Card)Activator.CreateInstance(type)!;
		card.BaseController = controller;
		card.ResetToBaseState();
		card.Init();
		card.isInitialized = true;
		return card;
	}

	public override void HandleNetworking()
	{
		while(true)
		{
			// Connect the players if they aren't yet
			if(playersConnected < players.Length && listener.Pending())
			{
				ConnectNewPlayer();
			}
			if(playersConnected == players.Length)
			{
				if(HandleGameLogic())
				{
					Log("Game ends by game logic");
					break;
				}
				if(HandlePlayerActions())
				{
					Log("Game ends by player action");
					break;
				}
			}
		}
	}

	private void ConnectNewPlayer()
	{
		DateTime t = DateTime.Now;
		Log($"{t.ToLongTimeString()}:{t.Millisecond} New Player {playersConnected}/{players.Length}");
		NetworkStream stream = listener.AcceptTcpClient().GetStream();
		byte[] buf = new byte[256];
		int len = stream.Read(buf, 0, HASH_LEN);
		if(len != HASH_LEN)
		{
			Log($"len was {len} but expected {HASH_LEN}\n-------------------\n{Encoding.UTF8.GetString(buf)}", severity: LogSeverity.Error);
			stream.Close();
			return;
		}
		string id = Encoding.UTF8.GetString(buf, 0, len);
		bool foundPlayer = false;
		for(int i = 0; i < players.Length; i++)
		{
			if(playerStreams[i] == null)
			{
				Log($"Player id: {players[i].id} ({players[i].id.Length}), found {id} ({id.Length}) | {players[i].id == id}");
				if(players[i].id == id)
				{
					playersConnected++;
					foundPlayer = true;
					playerStreams[i] = stream;
					stream.WriteByte((byte)i);
				}
			}
		}
		if(!foundPlayer)
		{
			Log("Found no player", severity: LogSeverity.Error);
			//FIXME: Be more nice, see above
			stream.Close();
		}
	}

	private void EvaluateLingeringEffects()
	{
		if(State == GameConstants.State.UNINITIALIZED)
		{
			return;
		}
		foreach(Player player in players)
		{
			player.ClearCardModifications();
		}
		SortedList<int, LingeringEffectInfo> infos = [];
		foreach(LingeringEffectInfo info in alwaysActiveLingeringEffects)
		{
			if(info.influenceLocation == GameConstants.Location.ALL)
			{
				if(info.timestamp == 0)
				{
					info.timestamp = LingeringEffectInfo.timestampCounter;
					LingeringEffectInfo.timestampCounter++;
				}
				infos.Add(info.timestamp, info);
			}
		}
		foreach(Player player in players)
		{
			foreach(Card card in player.hand.GetAll())
			{
				if(lingeringEffects.TryGetValue(card.uid, out LingeringEffectList? handInfos))
				{
					foreach(LingeringEffectInfo info in handInfos)
					{
						if(info.influenceLocation.HasFlag(card.Location))
						{
							if(info.timestamp == 0)
							{
								info.timestamp = LingeringEffectInfo.timestampCounter;
								LingeringEffectInfo.timestampCounter++;
							}
							infos.Add(info.timestamp, info);
						}
						else
						{
							info.timestamp = 0;
						}
					}
				}
				if(temporaryLingeringEffects.TryGetValue(card.uid, out LingeringEffectList? handTempInfos))
				{
					foreach(LingeringEffectInfo info in handTempInfos)
					{
						if(info.influenceLocation.HasFlag(card.Location))
						{
							if(info.timestamp == 0)
							{
								info.timestamp = LingeringEffectInfo.timestampCounter;
								LingeringEffectInfo.timestampCounter++;
							}
							infos.Add(info.timestamp, info);
						}
						else
						{
							info.timestamp = 0;
						}
					}
				}
			}
			foreach(Card card in player.field.GetUsed())
			{
				if(lingeringEffects.TryGetValue(card.uid, out LingeringEffectList? fieldInfos))
				{
					foreach(LingeringEffectInfo info in fieldInfos)
					{
						if(info.influenceLocation.HasFlag(card.Location))
						{
							if(info.timestamp == 0)
							{
								info.timestamp = LingeringEffectInfo.timestampCounter;
								LingeringEffectInfo.timestampCounter++;
							}
							infos.Add(info.timestamp, info);
						}
						else
						{
							info.timestamp = 0;
						}
					}
				}
				if(temporaryLingeringEffects.TryGetValue(card.uid, out LingeringEffectList? fieldTempInfos))
				{
					foreach(LingeringEffectInfo info in fieldTempInfos)
					{
						if(info.influenceLocation.HasFlag(card.Location))
						{
							if(info.timestamp == 0)
							{
								info.timestamp = LingeringEffectInfo.timestampCounter;
								LingeringEffectInfo.timestampCounter++;
							}
							infos.Add(info.timestamp, info);
						}
						else
						{
							info.timestamp = 0;
						}
					}
				}
			}
		}
		foreach(Player player in players)
		{
			if(lingeringEffects.TryGetValue(player.quest.uid, out LingeringEffectList? questInfos))
			{
				foreach(LingeringEffectInfo info in questInfos)
				{
					if(info.timestamp == 0)
					{
						info.timestamp = LingeringEffectInfo.timestampCounter;
						LingeringEffectInfo.timestampCounter++;
					}
					infos.Add(info.timestamp, info);
				}
			}
			if(temporaryLingeringEffects.TryGetValue(player.quest.uid, out LingeringEffectList? questTempInfos))
			{
				foreach(LingeringEffectInfo info in questTempInfos)
				{
					if(info.timestamp == 0)
					{
						info.timestamp = LingeringEffectInfo.timestampCounter;
						LingeringEffectInfo.timestampCounter++;
					}
					infos.Add(info.timestamp, info);
				}
			}
		}
		foreach(KeyValuePair<int, LingeringEffectInfo> info in infos)
		{
			info.Value.effect(info.Value.referrer);
			CheckQuestReward(false);
		}
		foreach(Player player in players)
		{
			for(int i = 0; i < GameConstants.FIELD_SIZE; i++)
			{
				Creature? card = player.field.GetByPosition(i);
				if(card != null && card.Life <= 0)
				{
					DestroyImpl(card);
				}
			}
		}
	}

	private void ProcessCreatureTargetingTriggers(Dictionary<int, List<CreatureTargetingTrigger>> triggers, Creature target, GameConstants.Location location, int uid)
	{
		if(triggers.TryGetValue(uid, out List<CreatureTargetingTrigger>? value))
		{
			foreach(CreatureTargetingTrigger trigger in value)
			{
				EvaluateLingeringEffects();
				if(trigger.influenceLocation.HasFlag(location) && trigger.condition(target))
				{
					trigger.effect(target);
					CheckQuestReward();
				}
			}
			EvaluateLingeringEffects();
		}
	}
	public void ProcessLocationBasedTargetingTriggers(Dictionary<int, List<LocationBasedTargetingTrigger>> triggers, Card target, int uid)
	{
		if(triggers.TryGetValue(uid, out List<LocationBasedTargetingTrigger>? matchingTriggers))
		{
			foreach(LocationBasedTargetingTrigger trigger in matchingTriggers)
			{
				EvaluateLingeringEffects();
				if(trigger.condition(target))
				{
					trigger.effect(target);
					CheckQuestReward();
				}
			}
			EvaluateLingeringEffects();
		}
	}
	public void ProcessLocationBasedTriggers(Dictionary<int, List<LocationBasedTrigger>> triggers, GameConstants.Location location, int uid)
	{
		if(triggers.TryGetValue(uid, out List<LocationBasedTrigger>? matchingTriggers))
		{
			for(int i = 0; i < matchingTriggers.Count; i++)
			{
				LocationBasedTrigger trigger = matchingTriggers[i];
				EvaluateLingeringEffects();
				if(trigger.influenceLocation.HasFlag(location) && trigger.condition())
				{
					trigger.effect();
					CheckQuestReward();
				}
			}
			EvaluateLingeringEffects();
		}
	}
	private void ProcessStateReachedTriggers()
	{
		// TODO: If this is slow, index by state?
		foreach(StateReachedTrigger trigger in alwaysActiveStateReachedTriggers)
		{
			EvaluateLingeringEffects();
			if(trigger.state == State && trigger.condition())
			{
				trigger.effect();
				trigger.wasTriggered = true;
				CheckQuestReward();
			}
		}
		alwaysActiveStateReachedTriggers.RemoveAll(card => card.oneshot && card.wasTriggered);
		EvaluateLingeringEffects();
		if(stateReachedTriggers.Count > 0)
		{
			foreach(Player player in players)
			{
				if(stateReachedTriggers.TryGetValue(player.quest.uid, out List<StateReachedTrigger>? questTriggers))
				{
					foreach(StateReachedTrigger trigger in questTriggers)
					{
						EvaluateLingeringEffects();
						if(trigger.state == State && trigger.condition())
						{
							trigger.effect();
							trigger.wasTriggered = true;
							CheckQuestReward();
						}
					}
					EvaluateLingeringEffects();
					questTriggers.RemoveAll(x => x.oneshot && x.wasTriggered);
				}

				foreach(Card card in player.hand.GetAll())
				{
					if(stateReachedTriggers.TryGetValue(card.uid, out List<StateReachedTrigger>? handTriggers))
					{
						foreach(StateReachedTrigger trigger in handTriggers)
						{
							EvaluateLingeringEffects();
							if(trigger.state == State && trigger.influenceLocation.HasFlag(GameConstants.Location.Hand) && trigger.condition())
							{
								trigger.effect();
								trigger.wasTriggered = true;
								CheckQuestReward();
							}
							else
							{
								trigger.wasTriggered = false;
							}
						}
						handTriggers.RemoveAll(x => x.oneshot && x.wasTriggered);
						EvaluateLingeringEffects();
					}
				}
				foreach(Card? card in player.field.GetAll())
				{
					if(card != null && stateReachedTriggers.TryGetValue(card.uid, out List<StateReachedTrigger>? fieldTriggers))
					{
						foreach(StateReachedTrigger trigger in fieldTriggers)
						{
							EvaluateLingeringEffects();
							if(trigger.state == State && trigger.influenceLocation.HasFlag(GameConstants.Location.Field) && trigger.condition())
							{
								trigger.effect();
								trigger.wasTriggered = true;
								CheckQuestReward();
							}
							else
							{
								trigger.wasTriggered = false;
							}
						}

						fieldTriggers.RemoveAll(x => x.oneshot && x.wasTriggered);
						EvaluateLingeringEffects();
					}
				}
			}
		}
	}
	public void ProcessTokenCreationTriggers(Dictionary<int, List<TokenCreationTrigger>> triggers, Creature token, Card source, int uid)
	{
		if(triggers.TryGetValue(uid, out List<TokenCreationTrigger>? matchintTriggers))
		{
			foreach(TokenCreationTrigger trigger in matchintTriggers)
			{
				EvaluateLingeringEffects();
				if(trigger.condition(token: token, source: source))
				{
					trigger.effect(token: token, source: source);
					CheckQuestReward();
				}
			}
			EvaluateLingeringEffects();
		}

	}
	public void ProcessTriggers(Dictionary<int, List<Trigger>> triggers, int uid)
	{
		if(triggers.TryGetValue(uid, out List<Trigger>? matchingTriggers))
		{
			foreach(Trigger trigger in matchingTriggers)
			{
				EvaluateLingeringEffects();
				if(trigger.condition())
				{
					trigger.effect();
					CheckQuestReward();
				}
			}
			EvaluateLingeringEffects();
		}
	}

	public void CheckQuestReward(bool shouldEvaluateLingeringEffects = true)
	{
		if(shouldEvaluateLingeringEffects)
		{
			EvaluateLingeringEffects();
		}
		foreach(Player p in players)
		{
			if(!rewardClaimed && p.quest.Progress >= p.quest.Goal)
			{
				rewardClaimed = true;
				p.quest.Reward();
				p.quest.Text += "\nREWARD CLAIMED";
				break;
			}
		}
	}
	private bool HandleGameLogic()
	{
		while(!State.HasFlag(GameConstants.State.InitGained))
		{
			if(State != GameConstants.State.UNINITIALIZED)
			{
				EvaluateLingeringEffects();
				for(int i = 0; i < players.Length; i++)
				{
					CheckIfLost(i);
					if(players[i].life <= 0)
					{
						return true;
					}
				}
			}
			switch(State)
			{
				case GameConstants.State.UNINITIALIZED:
				{
					foreach(Player player in players)
					{
						if(!Program.config.duel_config!.noshuffle)
						{
							player.deck.Shuffle();
						}
						player.Draw(GameConstants.START_HAND_SIZE);
						player.momentum = momentumBase;
						player.life = GameConstants.START_LIFE;
						player.discardCounts.Add(0);
						player.dealtDamages.Add(0);
						player.dealtSpellDamages.Add(0);
						player.brittleDeathCounts.Add(0);
						player.deathCounts.Add(0);
					}
					turnPlayer = rnd.Next(100) / 50;
					initPlayer = turnPlayer;
					turn = 0;
					SendFieldUpdates();
					// Mulligan
					for(int i = 0; i < players.Length; i++)
					{
						if(AskYesNoImpl(player: i, question: "Mulligan?"))
						{
							Card[] cards = SelectCardsCustom(i, "Select cards to mulligan", players[i].hand.GetAll(), (x) => true);
							foreach(Card card in cards)
							{
								players[i].hand.Remove(card);
								AddCardToLocation(card, GameConstants.Location.Deck);
							}
							players[i].deck.Shuffle();
							players[i].Draw(cards.Length);
							Log("Done with mulligan, sending updates now");
							SendFieldUpdates();
						}
					}
					State = GameConstants.State.TurnStart;
				}
				break;
				case GameConstants.State.TurnStart:
				{
					foreach(Player player in players)
					{
						player.abilityUsable = true;
						player.momentum = momentumBase;
						player.castCounts.Clear();
						player.Draw(1);
					}
					foreach(KeyValuePair<int, List<ActivatedEffectInfo>> lists in activatedEffects)
					{
						foreach(ActivatedEffectInfo list in lists.Value)
						{
							list.uses = 0;
						}
					}
					initPlayer = turnPlayer;
					ProcessStateReachedTriggers();
					State = GameConstants.State.MainInitGained;
				}
				break;
				case GameConstants.State.MainInitGained:
					break;
				case GameConstants.State.MainActionTaken:
				{
					initPlayer = 1 - initPlayer;
					players[initPlayer].passed = false;
					State = GameConstants.State.MainInitGained;
				}
				break;
				case GameConstants.State.BattleStart:
				{
					// The marked zone is relative to the 0th player
					// If player 1 is turnplayer it is FIELD_SIZE-1, the rightmost zone
					markedZone = turnPlayer * (GameConstants.FIELD_SIZE - 1);
					initPlayer = turnPlayer;
					foreach(Player player in players)
					{
						player.passed = false;
					}
					ProcessStateReachedTriggers();
					State = GameConstants.State.BattleInitGained;
				}
				break;
				case GameConstants.State.BattleInitGained:
					break;
				case GameConstants.State.BattleActionTaken:
				{
					initPlayer = 1 - initPlayer;
					players[initPlayer].passed = false;
					State = GameConstants.State.BattleInitGained;
				}
				break;
				case GameConstants.State.DamageCalc:
				{
					if(markedZone != null)
					{
						Creature? card0 = players[0].field.GetByPosition(GetMarkedZoneForPlayer(0));
						Creature? card1 = players[1].field.GetByPosition(GetMarkedZoneForPlayer(1));
						if(card0 != null)
						{
							ProcessTriggers(attackTriggers, card0.uid);
						}
						if(card1 != null)
						{
							ProcessTriggers(attackTriggers, card1.uid);
						}
						if(card0 == null)
						{
							if(card1 != null)
							{
								// Deal damage to player
								DealDamage(player: 0, amount: card1.Power, source: card1);
								if(players[0].life <= 0)
								{
									return true;
								}
							}
						}
						else
						{
							if(card1 == null)
							{
								DealDamage(player: 1, amount: card0.Power, source: card0);
								if(players[1].life <= 0)
								{
									return true;
								}
							}
							else
							{
								CreatureChangeLifeImpl(target: card0, amount: -card1.Power, source: card1);
								CreatureChangeLifeImpl(target: card1, amount: -card0.Power, source: card0);
								if(!card0.Location.HasFlag(GameConstants.Location.Field) && card1.Location.HasFlag(GameConstants.Location.Field))
								{
									ProcessTriggers(victoriousTriggers, card1.uid);
								}
								if(!card1.Location.HasFlag(GameConstants.Location.Field) && card0.Location.HasFlag(GameConstants.Location.Field))
								{
									ProcessTriggers(victoriousTriggers, card0.uid);
								}
							}
						}
					}
					foreach(Player player in players)
					{
						player.passed = false;
					}
					MarkNextZoneOrContinue();
				}
				break;
				case GameConstants.State.TurnEnd:
				{
					foreach(Player player in players)
					{
						for(int i = 0; i < GameConstants.FIELD_SIZE; i++)
						{
							Creature? c = player.field.GetByPosition(i);
							if(c != null)
							{
								if(c.Keywords.ContainsKey(Keyword.Brittle))
								{
									DestroyImpl(c);
								}
								if(c.Keywords.ContainsKey(Keyword.Decaying))
								{
									RegisterTemporaryLingeringEffectImpl(info: LingeringEffectInfo.Create(effect: (tg) => tg.Life -= 1, referrer: c));
									if(c.Life == 0 && c.Location.HasFlag(GameConstants.Location.Field))
									{
										DestroyImpl(c);
									}
								}
							}
						}
						player.discardCounts.Add(0);
						player.dealtDamages.Add(0);
						player.dealtSpellDamages.Add(0);
						player.brittleDeathCounts.Add(0);
						player.deathCounts.Add(0);
					}
					ProcessStateReachedTriggers();
					for(int i = 0; i < players.Length; i++)
					{
						Player player = players[i];
						int toDeckCount = player.hand.Count - GameConstants.MAX_HAND_SIZE;
						if(toDeckCount > 0)
						{
							SendPacketToPlayer(new DuelPackets.SelectCardsRequest
							{
								amount = toDeckCount,
								cards = Card.ToStruct(player.hand.GetAll()),
								desc = "Select cards to shuffle into you deck for hand size",
							}, i);
							int[] toHand = ReceivePacketFromPlayer<DuelPackets.SelectCardsResponse>(i).uids;
							foreach(int uid in toHand)
							{
								Card card = player.hand.GetByUID(uid);
								player.hand.Remove(card);
								player.deck.Add(card);
							}
							player.deck.Shuffle();
							SendFieldUpdates();
						}
					}
					turnPlayer = 1 - turnPlayer;
					turn++;
					if(nextMomentumIncreaseIndex < GameConstants.MOMENTUM_INCREMENT_TURNS.Length && GameConstants.MOMENTUM_INCREMENT_TURNS[nextMomentumIncreaseIndex] == turn)
					{
						momentumBase++;
						nextMomentumIncreaseIndex++;
					}
					State = GameConstants.State.TurnStart;
				}
				break;
				default:
					throw new NotImplementedException(State.ToString());
			}
			SendFieldUpdates();
		}
		return false;
	}

	private void CheckIfLost(int player)
	{
		if(players[player].life <= 0)
		{
			SendFieldUpdates();
			SendPacketToPlayer(new DuelPackets.GameResultResponse
			{
				result = GameConstants.GameResult.Lost
			}, player);
			SendPacketToPlayer(new DuelPackets.GameResultResponse
			{
				result = GameConstants.GameResult.Won
			}, 1 - player);
		}
	}
	public void CreatureChangeLifeImpl(Creature target, int amount, Card source)
	{
		if(amount == 0) return;
		if(amount < 0 && source.CardType == GameConstants.CardType.Spell)
		{
			players[source.Controller].dealtSpellDamages[turn] -= amount;
		}
		RegisterTemporaryLingeringEffectImpl(info: LingeringEffectInfo.Create(effect: (tg) => tg.Life += amount, referrer: target));
	}
	public void CreatureChangePowerImpl(Creature target, int amount, Card source)
	{
		if(amount == 0) return;
		RegisterTemporaryLingeringEffectImpl(info: LingeringEffectInfo.Create(effect: (tg) => tg.Power += amount, referrer: target));
	}

	private void DealDamage(int player, int amount, Card source)
	{
		players[player].life -= amount;
		players[1 - player].dealtDamages[turn] += amount;
		if(source.CardType == GameConstants.CardType.Spell)
		{
			players[1 - player].dealtSpellDamages[turn] += amount;

		}
		RevealImpl(player, amount);
		CheckIfLost(player);
		ProcessTriggers(dealsDamageTriggers, source.uid);
	}
	private void RevealImpl(int player, int damage)
	{
		for(int i = 0; i < Math.Min(damage, players[player].deck.Size); i++)
		{
			Card c = players[player].deck.GetAt(i);
			SendFieldUpdates(shownInfos: new() { { player, new() { card = c.ToStruct(), description = "Revealed" } } });
			ProcessTriggers(revelationTriggers, c.uid);
			SendFieldUpdates();
		}
		players[player].deck.Shuffle();
	}
	private void MarkNextZoneOrContinue()
	{
		if(markedZone == null)
		{
			State = GameConstants.State.TurnEnd;
			return;
		}
		if(turnPlayer == 0)
		{
			markedZone++;
			if(markedZone == GameConstants.FIELD_SIZE)
			{
				markedZone = null;
			}
		}
		else
		{
			markedZone--;
			if(markedZone < 0)
			{
				markedZone = null;
			}
		}
		initPlayer = turnPlayer;
		State = GameConstants.State.BattleInitGained;
	}
	private int GetMarkedZoneForPlayer(int player)
	{
		if(player == 0)
		{
			return markedZone!.Value;
		}
		else
		{
			return GameConstants.FIELD_SIZE - 1 - markedZone!.Value;
		}
		// Equivalent but magic:
		// return player * (GameConstants.FIELD_SIZE - 1 - 2 * markedZone!.Value) + markedZone!.Value;
	}

	private bool HandlePlayerActions()
	{
		for(int i = 0; i < players.Length; i++)
		{
			if(playerStreams[i]!.DataAvailable)
			{
				(byte typeByte, byte[]? bytes) = ReceiveRawPacket(playerStreams[i]!);
				if(bytes == null || bytes.Length == 0)
				{
					Log("Request was empty, ignoring it", severity: LogSeverity.Warning);
				}
				else
				{
					Program.replay?.actions.Add(new Replay.GameAction(packet: bytes, packetType: typeByte, player: i, clientToServer: true));
					if(HandlePacket(typeByte, bytes, i))
					{
						Log($"{players[i].name} is giving up, closing.");
						return true;
					}
				}
			}
			else
			{
				Thread.Sleep(10);
			}
		}
		return false;
	}

	private bool HandlePacket(byte typeByte, byte[] packet, int player)
	{
		// THIS MIGHT CHANGE AS SENDING RAW JSON MIGHT BE TOO EXPENSIVE/SLOW
		// possible improvements: Huffman or Burrows-Wheeler+RLE
		if(typeByte >= (byte)NetworkingConstants.PacketType.PACKET_COUNT)
		{
			throw new Exception($"ERROR: Unknown packet type encountered: ({typeByte})");
		}
		NetworkingConstants.PacketType type = (NetworkingConstants.PacketType)typeByte;

		switch(type)
		{
			case NetworkingConstants.PacketType.DuelSurrenderRequest:
			{
				SendPacketToPlayer(new DuelPackets.GameResultResponse
				{
					result = GameConstants.GameResult.Won
				}, 1 - player);
				Log("Surrender request received");
				return true;
			}
			case NetworkingConstants.PacketType.DuelGetOptionsRequest:
			{
				DuelPackets.GetOptionsRequest request = DeserializeJson<DuelPackets.GetOptionsRequest>(packet);
				SendPacketToPlayer(new DuelPackets.GetOptionsResponse
				{
					location = request.location,
					uid = request.uid,
					options = GetCardActions(player, request.uid, request.location),
				}, player);
			}
			break;
			case NetworkingConstants.PacketType.DuelSelectOptionRequest:
			{
				DuelPackets.SelectOptionRequest request = DeserializeJson<DuelPackets.SelectOptionRequest>(packet);
				bool found = false;
				foreach(string desc in GetCardActions(player, request.uid, request.location))
				{
					if(desc == request.desc)
					{
						TakeAction(player, request.uid, request.location, request.desc);
						found = true;
						break;
					}
				}
				if(!found)
				{
					Log("Tried to use an option that is not present for that card", severity: LogSeverity.Warning);
				}
				State &= ~GameConstants.State.InitGained;
				State |= GameConstants.State.ActionTaken;
			}
			break;
			case NetworkingConstants.PacketType.DuelPassRequest:
			{
				switch(State)
				{
					case GameConstants.State.MainInitGained:
					{
						if(!players[player].passed)
						{
							if(players[1 - player].passed)
							{
								State = GameConstants.State.BattleStart;
							}
							else
							{
								players[player].passed = true;
								State = GameConstants.State.MainActionTaken;
							}
						}
					}
					break;
					case GameConstants.State.BattleInitGained:
					{
						if(!players[player].passed)
						{
							if(players[1 - player].passed)
							{
								State = GameConstants.State.DamageCalc;
							}
							else
							{
								players[player].passed = true;
								State = GameConstants.State.BattleActionTaken;
							}
						}
					}
					break;
					default:
						Log($"Unable to pass in state {State}", severity: LogSeverity.Warning);
						break;
				}
			}
			break;
			case NetworkingConstants.PacketType.DuelViewGraveRequest:
			{
				bool opponent = DeserializeJson<DuelPackets.ViewGraveRequest>(packet).opponent;
				SendPacketToPlayer(new DuelPackets.ViewCardsResponse
				{
					cards = Card.ToStruct(players[opponent ? 1 - player : player].grave.GetAll()),
					message = $"Your {(opponent ? "opponent's" : "")} grave"
				}, player);
			}
			break;
			default:
				throw new Exception($"ERROR: Unable to process this packet: ({type}) | {packet}");
		}
		return false;
	}

	private void TakeAction(int player, int uid, GameConstants.Location location, string option)
	{
		if(player != initPlayer)
		{
			return;
		}
		players[player].passed = false;
		if(activatedEffects.TryGetValue(uid, out List<ActivatedEffectInfo>? matchingInfos))
		{
			foreach(ActivatedEffectInfo info in matchingInfos)
			{
				if(info.influenceLocation.HasFlag(location) && option == info.name)
				{
					info.effect();
					info.uses++;
					EvaluateLingeringEffects();
					SendFieldUpdates();
					return;
				}
			}
		}
		switch(location)
		{
			case GameConstants.Location.Hand:
			{
				Card card = players[player].hand.GetByUID(uid);
				if(option == CastActionDescription)
				{
					players[player].momentum -= card.Cost;
					CastImpl(player, card);
				}
				else
				{
					throw new NotImplementedException($"Scripted action {option}");
				}
			}
			break;
			case GameConstants.Location.Quest:
			{
				if(players[player].quest.Progress >= players[player].quest.Goal)
				{
					throw new NotImplementedException($"GetActions for ignition quests");
				}
			}
			break;
			case GameConstants.Location.Ability:
			{
				if(players[player].abilityUsable && players[player].momentum > 0 && castTriggers.ContainsKey(players[player].ability.uid))
				{
					SendFieldUpdates(shownInfos: new() { { player, new() { card = players[player].ability.ToStruct(), description = "Ability" } } });
					players[player].momentum--;
					players[player].abilityUsable = false;
					ProcessTriggers(castTriggers, players[player].ability.uid);
					foreach(Player p in players)
					{
						ProcessLocationBasedTargetingTriggers(triggers: genericCastTriggers, target: players[player].ability, uid: p.quest.uid);
						foreach(Card possiblyTriggeringCard in p.field.GetUsed())
						{
							ProcessLocationBasedTargetingTriggers(triggers: genericCastTriggers, target: players[player].ability, uid: possiblyTriggeringCard.uid);
						}
					}
				}
			}
			break;
			case GameConstants.Location.Field:
			{
				Creature card = players[player].field.GetByUID(uid);
				if(option == CreatureMoveActionDescription)
				{
					if(players[player].field.CanMove(card.Position, players[player].momentum))
					{
						int zone = SelectMovementZone(player, card.Position, players[player].momentum);
						players[player].momentum -= Math.Abs(card.Position - zone) * card.CalculateMovementCost();
						MoveImpl(card, zone);
					}
				}
				else
				{
					throw new NotImplementedException($"Scripted onfield option {option}");
				}
			}
			break;
			default:
				throw new NotImplementedException($"TakeAction at {location}");
		}
	}

	public void MoveImpl(Creature card, int zone)
	{
		players[card.Controller].field.Move(card.Position, zone);
		SendFieldUpdates();
	}

	private string[] GetCardActions(int player, int uid, GameConstants.Location location)
	{
		if(player != initPlayer)
		{
			return [];
		}
		EvaluateLingeringEffects();
		List<string> options = [];
		if(activatedEffects.TryGetValue(uid, out List<ActivatedEffectInfo>? matchingInfos))
		{
			foreach(ActivatedEffectInfo info in matchingInfos)
			{
				if(info.uses < info.maxUses && info.influenceLocation.HasFlag(location) && info.referrer.Location == location && info.condition())
				{
					options.Add(info.name);
				}
			}
		}
		switch(location)
		{
			case GameConstants.Location.Hand:
			{
				Card card = players[player].hand.GetByUID(uid);
				if(card.Cost <= players[player].momentum &&
					!(State.HasFlag(GameConstants.State.BattleStart) && card.CardType == GameConstants.CardType.Creature))
				{
					bool canCast = true;
					if(castTriggers.TryGetValue(card.uid, out List<Trigger>? matchingTriggers))
					{
						foreach(Trigger trigger in matchingTriggers)
						{
							EvaluateLingeringEffects();
							canCast = trigger.condition();
							if(!canCast)
							{
								break;
							}
						}
					}
					else
					{
						if(card.CardType == GameConstants.CardType.Spell)
						{
							canCast = false;
						}
					}
					if(canCast)
					{
						options.Add(CastActionDescription);
					}
				}
			}
			break;
			case GameConstants.Location.Ability:
			{
				if(players[player].abilityUsable && players[player].momentum > 0 && castTriggers.ContainsKey(players[player].ability.uid))
				{
					options.Add(AbilityUseActionDescription);
				}
			}
			break;
			case GameConstants.Location.Field:
			{
				Creature card = players[player].field.GetByUID(uid);
				if(players[player].field.CanMove(card.Position, players[player].momentum))
				{
					options.Add(CreatureMoveActionDescription);
				}
			}
			break;
			case GameConstants.Location.Quest:
				Log("Quests are not foreseen to have activated abilities", severity: LogSeverity.Warning);
				break;
			default:
				throw new NotImplementedException($"GetCardActions at {location}");
		}
		return [.. options];
	}

	public bool AskYesNoImpl(int player, string question)
	{
		Log("Asking yes no");
		SendPacketToPlayer(new DuelPackets.YesNoRequest { question = question }, player);
		Log("Receiving");
		return ReceivePacketFromPlayer<DuelPackets.YesNoResponse>(player).result;
	}
	private void SendFieldUpdates(Dictionary<int, DuelPackets.FieldUpdateRequest.Field.ShownInfo>? shownInfos = null)
	{
		EvaluateLingeringEffects();
		for(int i = 0; i < players.Length; i++)
		{
			SendFieldUpdate(i, shownInfos ?? []);
		}
	}
	private void SendFieldUpdate(int player, Dictionary<int, DuelPackets.FieldUpdateRequest.Field.ShownInfo> shownInfos)
	{
		// TODO: actually handle mask if this is too slow
		DuelPackets.FieldUpdateRequest request = new()
		{
			turn = turn + 1,
			hasInitiative = State != GameConstants.State.UNINITIALIZED && initPlayer == player,
			battleDirectionLeftToRight = player == turnPlayer,
			markedZone = player == 0 ? markedZone : (GameConstants.FIELD_SIZE - 1 - markedZone),
			ownField = new DuelPackets.FieldUpdateRequest.Field
			{
				ability = players[player].ability.ToStruct(),
				quest = players[player].quest.ToStruct(),
				deckSize = players[player].deck.Size,
				graveSize = players[player].grave.Size,
				life = players[player].life,
				name = players[player].name,
				momentum = players[player].momentum,
				field = players[player].field.ToStruct(),
				hand = players[player].hand.ToStruct(),
				shownInfo = shownInfos.GetValueOrDefault(player, new()),
			},
			oppField = new DuelPackets.FieldUpdateRequest.Field
			{
				ability = players[1 - player].ability.ToStruct(),
				quest = players[1 - player].quest.ToStruct(),
				deckSize = players[1 - player].deck.Size,
				graveSize = players[1 - player].grave.Size,
				life = players[1 - player].life,
				name = players[1 - player].name,
				momentum = players[1 - player].momentum,
				field = players[1 - player].field.ToStruct(),
				hand = players[1 - player].hand.ToHiddenStruct(),
				shownInfo = shownInfos.GetValueOrDefault(1 - player, new()),
			},
		};
		SendPacketToPlayer(request, player);
	}
	public Card[] SelectCardsCustom(int player, string description, Card[] cards, Func<Card[], bool> isValidSelection)
	{
		Log("Select cards custom");
		SendPacketToPlayer(new DuelPackets.CustomSelectCardsRequest
		{
			cards = Card.ToStruct(cards),
			desc = description,
			initialState = isValidSelection([])
		}, player);

		Log("request sent");
		byte type;
		byte[]? payload = null;
		do
		{
			DuelPackets.CustomSelectCardsIntermediateRequest request;
			(type, payload) = ReceiveRawPacket(playerStreams[player]!);
			Log("request received");
			Program.replay?.actions.Add(new Replay.GameAction(player: player, packetType: type, packet: payload, clientToServer: true));
			if(type == (byte)NetworkingConstants.PacketType.DuelCustomSelectCardsResponse)
			{
				Log("breaking out");
				break;
			}
			if(type != (byte)NetworkingConstants.PacketType.DuelCustomSelectCardsIntermediateRequest)
			{
				continue;
			}
			request = DeserializePayload<DuelPackets.CustomSelectCardsIntermediateRequest>(type, payload);
			Log("deserialized packet");
			SendPacketToPlayer(new DuelPackets.CustomSelectCardsIntermediateResponse
			{
				isValid = isValidSelection(Array.ConvertAll(request.uids, x => Array.Find(cards, y => y.uid == x)!))
			}, player);
			Log("sent packet");
		} while(true);

		DuelPackets.CustomSelectCardsResponse response = DeserializePayload<DuelPackets.CustomSelectCardsResponse>(type, payload);
		Log("final response");
		Card[] ret = UidsToCards(cards, response.uids);
		if(!isValidSelection(ret))
		{
			throw new Exception("Player somethow selected invalid cards");
		}
		Log("returning");
		return ret;
	}

	public static Card[] UidsToCards(Card[] cards, int[] uids)
	{
		Card[] ret = new Card[uids.Length];
		for(int i = 0; i < ret.Length; i++)
		{
			bool found = false;
			for(int j = 0; j < cards.Length; j++)
			{
				if(cards[j].uid == uids[i])
				{
					ret[i] = cards[j];
					found = true;
					break;
				}
			}
			if(!found)
			{
				throw new Exception($"Selected uid {uids[i]} could not be found in the source array");
			}
		}
		return ret;
	}
	private int SelectMovementZone(int player, int position, int momentum)
	{
		SendPacketToPlayer(new DuelPackets.SelectZoneRequest
		{
			options = players[player].field.GetMovementOptions(position, momentum),
		}, player);
		return ReceivePacketFromPlayer<DuelPackets.SelectZoneResponse>(player).zone;
	}
	private int GetCastCountImpl(int player, string name)
	{
		return players[player].castCounts.GetValueOrDefault(name, 0);
	}
	private int GetTurnImpl()
	{
		return turn;
	}
	private int GetPlayerLifeImpl(int player)
	{
		return players[player].life;
	}
	private void DrawImpl(int player, int amount)
	{
		players[player].Draw(amount);
		SendFieldUpdates();
	}
	private void MoveToFieldImpl(int choosingPlayer, int targetPlayer, Creature creature, Card? source)
	{
		EvaluateLingeringEffects();
		bool wasAlreadyOnField = creature.Location == GameConstants.Location.Field;
		RemoveCardFromItsLocation(creature);
		int zone = SelectZoneImpl(choosingPlayer: choosingPlayer, targetPlayer: targetPlayer);
		if(creature.Controller != targetPlayer)
		{
			RegisterControllerChange(creature);
		}
		players[targetPlayer].field.Add(creature, zone);
		RemoveOutdatedTemporaryLingeringEffects(creature);
		if(!wasAlreadyOnField)
		{
			foreach(Player p in players)
			{
				ProcessLocationBasedTargetingTriggers(genericEnterFieldTriggers, target: creature, uid: p.quest.uid);
				foreach(Creature possiblyTriggeringCard in p.field.GetUsed())
				{
					ProcessLocationBasedTargetingTriggers(genericEnterFieldTriggers, target: creature, uid: possiblyTriggeringCard.uid);
				}
				if(creature.Keywords.ContainsKey(Keyword.Token))
				{
					if(source == null)
					{
						throw new Exception($"Moving token {creature.Name} to field but source was null");
					}
					ProcessTokenCreationTriggers(tokenCreationTriggers, token: creature, source: source, uid: p.quest.uid);
					foreach(Creature possiblyTriggeringCard in p.field.GetUsed())
					{
						ProcessTokenCreationTriggers(tokenCreationTriggers, token: creature, source: source, uid: possiblyTriggeringCard.uid);
					}
				}
			}
		}
	}
	private void CastImpl(int player, Card card)
	{
		EvaluateLingeringEffects();
		bool isNew = !card.isInitialized;
		if(isNew)
		{
			card.Init();
			card.isInitialized = true;
		}
		RemoveCardFromItsLocation(card);
		SendFieldUpdates(shownInfos: new() { { player, new() { card = card.ToStruct(), description = CastActionDescription } } });
		if(!isNew)
		{
			switch(card.CardType)
			{
				case GameConstants.CardType.Creature:
				{
					MoveToFieldImpl(player, player, (Creature)card, null);
				}
				break;
				case GameConstants.CardType.Spell:
				{
					AddCardToLocation(card, GameConstants.Location.Grave);
				}
				break;
				default:
					throw new NotImplementedException($"Casting {card.CardType} cards");
			}
		}
		if(!players[player].castCounts.TryGetValue(card.Name, out int value))
		{
			value = 0;
			players[player].castCounts[card.Name] = value;
		}
		players[player].castCounts[card.Name] = ++value;
		ProcessTriggers(castTriggers, card.uid);
		foreach(Player p in players)
		{
			ProcessLocationBasedTargetingTriggers(genericCastTriggers, target: card, uid: p.quest.uid);
			foreach(Card possiblyTriggeringCard in p.field.GetUsed())
			{
				ProcessLocationBasedTargetingTriggers(genericCastTriggers, target: card, uid: possiblyTriggeringCard.uid);
			}
		}
		SendFieldUpdates();
	}

	public void ResetAbilityImpl(int player)
	{
		players[player].abilityUsable = true;
	}

	public void RegisterCastTriggerImpl(Trigger trigger, Card referrer)
	{
		castTriggers.TryAdd(referrer.uid, []);
		castTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterDealsDamageTriggerImpl(Trigger trigger, Card referrer)
	{
		dealsDamageTriggers.TryAdd(referrer.uid, []);
		dealsDamageTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterGenericCastTriggerImpl(LocationBasedTargetingTrigger trigger, Card referrer)
	{
		genericCastTriggers.TryAdd(referrer.uid, []);
		genericCastTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterRevelationTriggerImpl(Trigger trigger, Card referrer)
	{
		revelationTriggers.TryAdd(referrer.uid, []);
		revelationTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterGenericEntersFieldTriggerImpl(LocationBasedTargetingTrigger trigger, Card referrer)
	{
		genericEnterFieldTriggers.TryAdd(referrer.uid, []);
		genericEnterFieldTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterYouDiscardTriggerImpl(LocationBasedTrigger trigger, Card referrer)
	{
		youDiscardTriggers.TryAdd(referrer.uid, []);
		youDiscardTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterDiscardTriggerImpl(Trigger trigger, Card referrer)
	{
		discardTriggers.TryAdd(referrer.uid, []);
		discardTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterStateReachedTriggerImpl(StateReachedTrigger trigger, Card referrer)
	{
		if(trigger.influenceLocation == GameConstants.Location.ALL)
		{
			alwaysActiveStateReachedTriggers.Add(trigger);
		}
		else
		{
			stateReachedTriggers.TryAdd(referrer.uid, []);
			stateReachedTriggers[referrer.uid].Add(trigger);
		}
	}
	public void RegisterLingeringEffectImpl(LingeringEffectInfo info)
	{
		if(info.influenceLocation == GameConstants.Location.ALL)
		{
			alwaysActiveLingeringEffects.Add(info);
		}
		else
		{
			lingeringEffects.TryAdd(info.referrer.uid, new(this));
			lingeringEffects[info.referrer.uid].Add(info);
		}
	}
	public void RegisterTemporaryLingeringEffectImpl(LingeringEffectInfo info)
	{
		temporaryLingeringEffects.TryAdd(info.referrer.uid, new(this));
		temporaryLingeringEffects[info.referrer.uid].Add(info);
	}
	public void RegisterActivatedEffectImpl(ActivatedEffectInfo info)
	{
		activatedEffects.TryAdd(info.referrer.uid, []);
		activatedEffects[info.referrer.uid].Add(info);
	}
	public void RegisterVictoriousTriggerImpl(Trigger trigger, Card referrer)
	{
		victoriousTriggers.TryAdd(referrer.uid, []);
		victoriousTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterAttackTriggerImpl(Trigger trigger, Card referrer)
	{
		attackTriggers.TryAdd(referrer.uid, []);
		attackTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterDeathTriggerImpl(CreatureTargetingTrigger trigger, Card referrer)
	{
		deathTriggers.TryAdd(referrer.uid, []);
		deathTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterGenericDeathTriggerImpl(CreatureTargetingTrigger trigger, Card referrer)
	{
		genericDeathTriggers.TryAdd(referrer.uid, []);
		genericDeathTriggers[referrer.uid].Add(trigger);
	}
	private void RegisterTokenCreationTriggerImpl(TokenCreationTrigger trigger, Card referrer)
	{
		tokenCreationTriggers.TryAdd(referrer.uid, []);
		tokenCreationTriggers[referrer.uid].Add(trigger);
	}
	public Creature?[] GetFieldImpl(int player)
	{
		return players[player].field.GetAll();
	}
	public Creature[] GetFieldUsedImpl(int player)
	{
		return players[player].field.GetUsed();
	}
	public Card[] GetGraveImpl(int player)
	{
		return players[player].grave.GetAll();
	}
	public Card[] GetHandImpl(int player)
	{
		return players[player].hand.GetAll();
	}
	public void PlayerChangeLifeImpl(int player, int amount, Card source)
	{
		if(amount > 0)
		{
			players[player].life += amount;
		}
		else
		{
			DealDamage(player: player, amount: -amount, source: source);
		}
	}
	public Card GatherImpl(int player, int amount)
	{
		Card[] possibleCards = players[player].deck.GetRange(0, amount);
		Card target = CardUtils.SelectSingleCard(player: player, cards: possibleCards, description: "Select card to gather");
		MoveToHandImpl(player, target);
		players[player].deck.Shuffle();
		return target;
	}
	public void PayLifeImpl(int player, int amount)
	{
		players[player].life -= amount;
		CheckIfLost(player);
	}
	public int GetIgniteDamageImpl(int player)
	{
		EvaluateLingeringEffects();
		return players[player].igniteDamage;
	}
	public void ChangeIgniteDamageImpl(int player, int amount)
	{
		players[player].baseIgniteDamage += amount;
	}
	public void ChangeIgniteDamageTemporaryImpl(int player, int amount)
	{
		players[player].igniteDamage += amount;
	}
	public void PlayerChangeMomentumImpl(int player, int amount)
	{
		players[player].momentum += amount;
		if(players[player].momentum < 0) players[player].momentum = 0;
	}
	public void MoveToHandImpl(int player, Card card)
	{
		EvaluateLingeringEffects();
		switch(card.Location)
		{
			case GameConstants.Location.Deck:
				players[card.Controller].deck.Remove(card);
				break;
			case GameConstants.Location.Hand:
			{
				if(card.Controller == player)
				{
					Log($"Tried to add {card.Name} from the hand to the same hand", severity: LogSeverity.Warning);
				}
				else
				{
					players[card.Controller].hand.Remove(card);
				}
			}
			break;
			case GameConstants.Location.Field:
				players[card.Controller].field.Remove((Creature)card);
				break;
			case GameConstants.Location.Grave:
				players[card.Controller].grave.Remove(card);
				break;
			default:
				throw new Exception($"Cannot add a card from {card.Location} to hand");
		}
		if(!(card.CardType == GameConstants.CardType.Creature && ((Creature)card).Keywords.ContainsKey(Keyword.Token)))
		{
			if(card.Controller != player)
			{
				RegisterControllerChange(card);
			}
			players[player].hand.Add(card);
		}
		else
		{
			Log($"Tried to add a token to hand", severity: LogSeverity.Warning);
		}
		RemoveOutdatedTemporaryLingeringEffects(card);
	}
	public Card[] GetDiscardableImpl(int player, Card? ignore)
	{
		return players[player].hand.GetDiscardable(ignore);
	}
	private void RegisterControllerChange(Card card, GameConstants.Location influenceLocation = ~(GameConstants.Location.Grave | GameConstants.Location.Deck))
	{
		RegisterTemporaryLingeringEffectImpl(info: LingeringEffectInfo.Create(effect: (target) => target.Controller = 1 - target.Controller, referrer: card, influenceLocation: influenceLocation));
	}
	public void DestroyImpl(Creature card)
	{
		switch(card.Location)
		{
			case GameConstants.Location.Field:
			{
				players[card.Controller].field.Remove(card);
				AddCardToLocation(card, GameConstants.Location.Grave);
			}
			break;
			case GameConstants.Location.UNKNOWN:
			{
				Log($"Destroying {card.Name} at UNKNOWN", severity: LogSeverity.Warning);
			}
			break;
			default:
				throw new Exception($"Destroying {card.Name} at {card.Location} is not supported");
		}
		if(card.Keywords.ContainsKey(Keyword.Brittle))
		{
			players[card.Controller].brittleDeathCounts[turn]++;
		}
		players[card.Controller].deathCounts[turn]++;
		ProcessCreatureTargetingTriggers(deathTriggers, target: card, uid: card.uid, location: card.Location);
		SendFieldUpdates();
		foreach(Player player in players)
		{
			foreach(Card fieldCard in player.field.GetUsed())
			{
				ProcessCreatureTargetingTriggers(genericDeathTriggers, target: card, uid: fieldCard.uid, location: GameConstants.Location.Field);
			}
			foreach(Card graveCard in player.grave.GetAll())
			{
				ProcessCreatureTargetingTriggers(genericDeathTriggers, target: card, uid: graveCard.uid, location: GameConstants.Location.Grave);
			}
			foreach(Card handCard in player.hand.GetAll())
			{
				ProcessCreatureTargetingTriggers(genericDeathTriggers, target: card, uid: handCard.uid, location: GameConstants.Location.Hand);
			}
		}
		foreach(Player player in players)
		{
			ProcessCreatureTargetingTriggers(triggers: genericDeathTriggers, target: card, location: GameConstants.Location.Quest, uid: player.quest.uid);
		}
	}
	private void RemoveOutdatedTemporaryLingeringEffects(Card card)
	{
		temporaryLingeringEffects.GetValueOrDefault(card.uid)?.RemoveAll(info => !info.influenceLocation.HasFlag(card.Location));
	}
	public bool RemoveCardFromItsLocation(Card card)
	{
		switch(card.Location)
		{
			case GameConstants.Location.Hand:
				players[card.Controller].hand.Remove(card);
				break;
			case GameConstants.Location.Field:
				players[card.Controller].field.Remove((Creature)card);
				break;
			case GameConstants.Location.Grave:
				players[card.Controller].grave.Remove(card);
				break;
			case GameConstants.Location.Deck:
				players[card.Controller].deck.Remove(card);
				break;
			default:
				return false;
		}
		return true;
	}
	public void ReturnCardsToDeckImpl(Card[] cards)
	{
		EvaluateLingeringEffects();
		bool[] shouldShuffle = new bool[players.Length];
		foreach(Card card in cards)
		{
			if(RemoveCardFromItsLocation(card))
			{
				shouldShuffle[card.BaseController] = true;
				AddCardToLocation(card, GameConstants.Location.Deck);
			}
			else
			{
				Log($"Could not move {card} from {card.Location} to deck.");
			}
		}
		for(int i = 0; i < shouldShuffle.Length; i++)
		{
			if(shouldShuffle[i])
			{
				players[i].deck.Shuffle();
			}
		}
	}
	public Card[] SelectCardsImpl(int player, Card[] cards, int amount, string description)
	{
		if(cards.Length < amount)
		{
			throw new Exception($"Tried to let a player select from a too small collection ({cards.Length} < {amount})");
		}
		SendPacketToPlayer(new DuelPackets.SelectCardsRequest
		{
			amount = amount,
			cards = Card.ToStruct(cards),
			desc = description,
		}, player);
		DuelPackets.SelectCardsResponse response = ReceivePacketFromPlayer<DuelPackets.SelectCardsResponse>(player);
		if(response.uids.Length != amount)
		{
			throw new Exception($"Selected the wrong amount of cards ({response.uids.Length} != {amount})");
		}
		// TODO: Make this nicer?
		return UidsToCards(cards, response.uids);
	}
	public void DiscardAmountImpl(int player, int amount)
	{
		Card[] cards = players[player].hand.GetDiscardable(null);
		amount = Math.Min(cards.Length, amount);
		if(amount == 0)
		{
			return;
		}
		Card[] targets = SelectCardsImpl(player: player, amount: amount, cards: cards, description: "Select cards to discard");
		foreach(Card target in targets)
		{
			DiscardImpl(target);
		}
	}
	private void AddCardToLocation(Card card, GameConstants.Location location)
	{
		if(card.BaseController < 0)
		{
			throw new Exception($"Tried to add a '{card.Name}' to {location} but BaseController was negative");
		}
		switch(location)
		{
			case GameConstants.Location.Deck:
			{
				players[card.BaseController].deck.Add(card);
			}
			break;
			case GameConstants.Location.Grave:
			{
				players[card.BaseController].grave.Add(card);
			}
			break;
			default:
			{
				throw new Exception($"Tried to add card {card.Name} to location {location} of {card.BaseController}");
			}
		}
		RemoveOutdatedTemporaryLingeringEffects(card);
	}
	public void DiscardImpl(Card card)
	{
		EvaluateLingeringEffects();
		if(card.Location != GameConstants.Location.Hand || !card.CanBeDiscarded())
		{
			throw new Exception($"Tried to discard a card that is not in the hand but at {card.Location}");
		}
		players[card.Controller].hand.Remove(card);
		AddCardToLocation(card, GameConstants.Location.Grave);
		players[card.Controller].discardCounts[turn]++;
		Player player = players[card.Controller];
		ProcessTriggers(discardTriggers, uid: card.uid);
		ProcessLocationBasedTriggers(youDiscardTriggers, GameConstants.Location.Quest, player.quest.uid);
		foreach(Card c in player.hand.GetAll())
		{
			ProcessLocationBasedTriggers(youDiscardTriggers, GameConstants.Location.Hand, uid: c.uid);
		}
		foreach(Creature c in player.field.GetUsed())
		{
			ProcessLocationBasedTriggers(youDiscardTriggers, GameConstants.Location.Field, uid: c.uid);
		}
	}
	public void CreateTokenOnFieldImpl(int player, int power, int life, string name, Card source)
	{
		MoveToFieldImpl(player, player, CreateTokenImpl(player, power, life, name), source);
	}
	public Token CreateTokenImpl(int player, int power, int life, string name)
	{
		if(!players[player].field.HasEmpty())
		{
			throw new Exception($"Tried to create a token but the field is full");
		}
		Token token = new
		(
			Name: name,
			Text: "",
			OriginalCost: 0,
			OriginalLife: life,
			OriginalPower: power,
			OriginalController: player
		);
		return token;
	}
	public void CreateTokenCopyOnFieldImpl(int player, Creature card, Card source)
	{
		MoveToFieldImpl(player, player, CreateTokenCopyImpl(player, card), source);
	}
	public Creature CreateTokenCopyImpl(int player, Creature card)
	{
		if(!players[player].field.HasEmpty())
		{
			throw new Exception($"Tried to create a token but the field is full");
		}
		Creature token;
		if(card.GetType() == typeof(Token))
		{
			token = CreateTokenImpl(player: player, power: card.Power, life: card.Life, name: card.Name);
			foreach(Keyword keyword in card.Keywords.Keys)
			{
				token.RegisterKeyword(keyword, card.Keywords[keyword]);
			}
		}
		else
		{
			token = (Creature)CreateBasicCard(card.GetType(), player);
			token.RegisterKeyword(Keyword.Token);
		}
		return token;
	}
	public int SelectZoneImpl(int choosingPlayer, int targetPlayer)
	{
		bool[] options = players[targetPlayer].field.GetPlacementOptions();
		if(choosingPlayer != targetPlayer)
		{
			Array.Reverse(options);
		}
		SendPacketToPlayer(new DuelPackets.SelectZoneRequest
		{
			options = options,
		}, choosingPlayer);
		int zone = ReceivePacketFromPlayer<DuelPackets.SelectZoneResponse>(choosingPlayer).zone;
		if(choosingPlayer != targetPlayer)
		{
			zone = GameConstants.FIELD_SIZE - zone - 1;
		}
		return zone;
	}
	public int GetDiscardCountXTurnsAgoImpl(int player, int turns)
	{
		if(turn < turns || players[player].discardCounts.Count <= turn - turns)
		{
			Log($"Attempted to get discard count before the game began ({turn - turns}) for player {players[player].name}", severity: LogSeverity.Warning);
			return 0;
		}
		return players[player].discardCounts[turn - turns];
	}

	public int GetDamageDealtXTurnsAgoImpl(int player, int turns)
	{
		if(turn < turns || players[player].dealtDamages.Count <= turn - turns)
		{
			Log($"Attempted to get damage dealt before the game began ({turn - turns}) for player {players[player].name}", severity: LogSeverity.Warning);
			return 0;
		}
		return players[player].dealtDamages[turn - turns];
	}
	public int GetSpellDamageDealtXTurnsAgoImpl(int player, int turns)
	{
		if(turn < turns || players[player].dealtSpellDamages.Count <= turn - turns)
		{
			Log($"Attempted to get spell damage dealt before the game began ({turn - turns}) for player {players[player].name}", severity: LogSeverity.Warning);
			return 0;
		}
		return players[player].dealtSpellDamages[turn - turns];
	}
	public int GetBrittleDeathCountXTurnsAgoImpl(int player, int turns)
	{
		if(turn < turns || players[player].brittleDeathCounts.Count <= turn - turns)
		{
			Log($"Attempted to get brittle death count before the game began ({turn - turns}) for player {players[player].name}", severity: LogSeverity.Warning);
			return 0;
		}
		return players[player].brittleDeathCounts[turn - turns];
	}
	public int GetDeathCountXTurnsAgoImpl(int player, int turns)
	{
		if(turn < turns || players[player].deathCounts.Count <= turn - turns)
		{
			Log($"Attempted to get death count before the game began ({turn - turns}) for player {players[player].name}", severity: LogSeverity.Warning);
			return 0;
		}
		return players[player].deathCounts[turn - turns];
	}

	public static T ReceivePacketFromPlayer<T>(int player) where T : PacketContent
	{
		byte[]? payload = ReceivePacket<T>(playerStreams[player]!);
		Program.replay?.actions.Add(new Replay.GameAction(player: player, packetType: NetworkingConstants.PacketDict[typeof(T)], packet: payload, clientToServer: true));
		return (payload == null) ? (T)new PacketContent() : DeserializeJson<T>(payload);
	}
	public static void SendPacketToPlayer<T>(T packet, int player) where T : PacketContent
	{
		byte[] payload = GeneratePayload(packet);
		Program.replay?.actions.Add(new Replay.GameAction(player: player, packetType: NetworkingConstants.PacketDict[typeof(T)], packet: payload[5..], clientToServer: false));
		playerStreams[player]!.Write(payload);
	}
}
