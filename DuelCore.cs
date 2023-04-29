using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using CardGameUtils;
using CardGameUtils.Structs;
using static CardGameUtils.Functions;
using static CardGameUtils.Structs.NetworkingStructs;

namespace CardGameCore;
// TODO: Everywhere where triggers are used, process opponent's responses instead of just executing the effect

class DuelCore : Core
{
	private static GameConstants.State _state = GameConstants.State.UNINITIALIZED;
	public static GameConstants.State state
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
	private SHA384 sha;
	public Player[] players;
	public static NetworkStream[] playerStreams = new NetworkStream[0];
	public static Random rnd = new Random(Program.seed);
	public const int HASH_LEN = 96;
	public int playersConnected = 0;
	public int turn, turnPlayer, initPlayer;
	public int? markedZone = null;
	public int momentumBase = GameConstants.START_MOMENTUM;
	public bool rewardClaimed = false;

	private Dictionary<int, List<CastTrigger>> castTriggers = new Dictionary<int, List<CastTrigger>>();
	private Dictionary<int, List<GenericCastTrigger>> genericCastTriggers = new Dictionary<int, List<GenericCastTrigger>>();
	private Dictionary<int, List<RevelationTrigger>> revelationTriggers = new Dictionary<int, List<RevelationTrigger>>();
	private Dictionary<int, List<Trigger>> victoriousTriggers = new Dictionary<int, List<Trigger>>();
	private Dictionary<int, List<Trigger>> attackTriggers = new Dictionary<int, List<Trigger>>();
	private Dictionary<int, List<Trigger>> deathTriggers = new Dictionary<int, List<Trigger>>();
	private Dictionary<int, List<GenericDeathTrigger>> genericDeathTriggers = new Dictionary<int, List<GenericDeathTrigger>>();
	private Dictionary<int, List<DiscardTrigger>> youDiscardTriggers = new Dictionary<int, List<DiscardTrigger>>();
	private Dictionary<int, List<DiscardTrigger>> discardTriggers = new Dictionary<int, List<DiscardTrigger>>();
	private Dictionary<int, List<StateReachedTrigger>> stateReachedTriggers = new Dictionary<int, List<StateReachedTrigger>>();
	private Dictionary<int, List<LingeringEffectInfo>> lingeringEffects = new Dictionary<int, List<LingeringEffectInfo>>();
	private Dictionary<int, List<LingeringEffectInfo>> temporaryLingeringEffects = new Dictionary<int, List<LingeringEffectInfo>>();
	private List<LingeringEffectInfo> alwaysActiveLingeringEffects = new List<LingeringEffectInfo>();
	private Dictionary<int, List<ActivatedEffectInfo>> activatedEffects = new Dictionary<int, List<ActivatedEffectInfo>>();
	private Dictionary<int, List<Trigger>> dealsDamageTriggers = new Dictionary<int, List<Trigger>>();

	public DuelCore()
	{
		RegisterScriptingFunctions();
		sha = SHA384.Create();
		players = new Player[Program.config.duel_config!.players.Length];
		playerStreams = new NetworkStream[Program.config.duel_config.players.Length];
		for(int i = 0; i < players.Length; i++)
		{
			Log("Player created. ID: " + Program.config.duel_config.players[i].id);
			Deck deck = new Deck();
			GameConstants.PlayerClass playerClass = Enum.Parse<GameConstants.PlayerClass>(Program.config.duel_config.players[i].decklist[0]);
			if(!Program.config.duel_config.players[i].decklist[1].StartsWith("#"))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no ability, {Program.config.duel_config.players[i].decklist[1]} is no suitable ability");
				return;
			}
			Card ability = CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[1].Substring(1))!, i);
			if(!Program.config.duel_config.players[i].decklist[2].StartsWith("|"))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no quest, {Program.config.duel_config.players[i].decklist[2]} is no suitable ability");
				return;
			}
			Quest quest = (Quest)CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[2].Substring(1))!, i);
			for(int j = 3; j < Program.config.duel_config.players[i].decklist.Length; j++)
			{
				Log($"Creating {Program.config.duel_config.players[i].decklist[j]}");
				deck.Add(CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[j])!, i));
			}
			players[i] = new Player(Program.config.duel_config.players[i], i, deck, playerClass, ability, quest);
		}
	}

	public void RegisterScriptingFunctions()
	{
		Card.RegisterCastTrigger = RegisterCastTriggerImpl;
		Card.RegisterGenericCastTrigger = RegisterGenericCastTriggerImpl;
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
		Card.GetGrave = GetGraveImpl;
		Card.GetField = GetFieldImpl;
		Card.GetFieldUsed = GetFieldUsedImpl;
		Card.GetHand = GetHandImpl;
		Card.SelectCards = SelectCardsImpl;
		Card.Discard = DiscardImpl;
		Card.DiscardAmount = DiscardAmountImpl;
		Card.CreateToken = CreateTokenImpl;
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
	}

	public override void Init()
	{
		HandleNetworking();
		foreach(NetworkStream stream in playerStreams)
		{
			if(stream != null)
			{
				stream.Dispose();
			}
		}
		listener.Stop();
	}

	private Card CreateBasicCard(Type type, int controller)
	{
		Card c = (Card)Activator.CreateInstance(type)!;
		c.Controller = controller;
		c.Init();
		c.isInitialized = true;
		return c;
	}

	public override void HandleNetworking()
	{
		listener.Start();
		while(true)
		{
			// Connect the players if they aren't yet
			if(playersConnected < players.Length && listener.Pending())
			{
				NetworkStream stream = listener.AcceptTcpClient().GetStream();
				byte[] buf = new byte[256];
				int len = stream.Read(buf, 0, HASH_LEN);
				if(len != HASH_LEN)
				{
					Log($"len was {len} but expected {HASH_LEN}\n-------------------\n{Encoding.UTF8.GetString(buf)}", severity: LogSeverity.Error);
					//FIXME: Be more nice than simply killing the connection
					stream.Close();
					continue;
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
				DateTime t = DateTime.Now;
				Log($"{t.ToLongTimeString()}:{t.Millisecond} New Player {playersConnected}/{players.Length}");
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
			//FIXME: Be more nice than simply killing the connection
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
		foreach(Player player in players)
		{
			player.ClearCardModifications();
		}
		SortedList<int, LingeringEffectInfo> infos = new SortedList<int, LingeringEffectInfo>();
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
				if(lingeringEffects.ContainsKey(card.uid))
				{
					foreach(LingeringEffectInfo info in lingeringEffects[card.uid])
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
				if(temporaryLingeringEffects.ContainsKey(card.uid))
				{
					foreach(LingeringEffectInfo info in temporaryLingeringEffects[card.uid])
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
				if(lingeringEffects.ContainsKey(card.uid))
				{
					foreach(LingeringEffectInfo info in lingeringEffects[card.uid])
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
				if(temporaryLingeringEffects.ContainsKey(card.uid))
				{
					foreach(LingeringEffectInfo info in temporaryLingeringEffects[card.uid])
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
			if(lingeringEffects.ContainsKey(player.quest.uid))
			{
				foreach(LingeringEffectInfo info in lingeringEffects[player.quest.uid])
				{
					if(info.timestamp == 0)
					{
						info.timestamp = LingeringEffectInfo.timestampCounter;
						LingeringEffectInfo.timestampCounter++;
					}
					infos.Add(info.timestamp, info);
				}
			}
			if(temporaryLingeringEffects.ContainsKey(player.quest.uid))
			{
				foreach(LingeringEffectInfo info in temporaryLingeringEffects[player.quest.uid])
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
			foreach(Player player in players)
			{
				if(!rewardClaimed && player.quest.Progress >= player.quest.Goal)
				{
					player.quest.Reward();
					rewardClaimed = true;
				}
			}
		}
		foreach(Player player in players)
		{
			for(int i = 0; i < GameConstants.FIELD_SIZE; i++)
			{
				Card? card = player.field.GetByPosition(i);
				if(card != null && card.Life <= 0)
				{
					DestroyImpl(card);
				}
			}
		}
	}

	public void ProcessTriggers<T>(Dictionary<int, List<T>> triggers, int uid) where T : Trigger
	{
		if(triggers.ContainsKey(uid))
		{
			foreach(T trigger in triggers[uid])
			{
				if(trigger.condition())
				{
					trigger.effect();
				}
			}
		}
	}

	private bool HandleGameLogic()
	{
		while(!state.HasFlag(GameConstants.State.InitGained))
		{
			if(state != GameConstants.State.UNINITIALIZED)
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
			switch(state)
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
								players[i].deck.Add(card);
							}
							players[i].deck.Shuffle();
							players[i].Draw(cards.Length);
							Log("Done with mulligan, sending updates now");
							SendFieldUpdates();
						}
					}
					state = GameConstants.State.TurnStart;
				}
				break;
				case GameConstants.State.TurnStart:
				{
					foreach(Player player in players)
					{
						player.ability.Position = 0;
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
					state = GameConstants.State.MainInitGained;
				}
				break;
				case GameConstants.State.MainInitGained:
					break;
				case GameConstants.State.MainActionTaken:
				{
					initPlayer = 1 - initPlayer;
					state = GameConstants.State.MainInitGained;
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
					state = GameConstants.State.BattleInitGained;
				}
				break;
				case GameConstants.State.BattleInitGained:
					break;
				case GameConstants.State.BattleActionTaken:
				{
					initPlayer = 1 - initPlayer;
					state = GameConstants.State.BattleInitGained;
				}
				break;
				case GameConstants.State.DamageCalc:
				{
					if(markedZone == null)
					{
						throw new Exception("No zone marked during damage calc");
					}
					Card? card0 = players[0].field.GetByPosition(GetMarkedZoneForPlayer(0));
					Card? card1 = players[1].field.GetByPosition(GetMarkedZoneForPlayer(1));
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
							RegisterTemporaryLingeringEffectImpl(info: new LingeringEffectInfo(effect: (target) => target.Life -= card1.Power, referrer: card0));
							RegisterTemporaryLingeringEffectImpl(info: new LingeringEffectInfo(effect: (target) => target.Life -= card0.Power, referrer: card1));
							EvaluateLingeringEffects();
							if(card0.Life == 0 && card1.Life != 0)
							{
								ProcessTriggers(victoriousTriggers, card1.uid);
							}
							if(card1.Life == 0 && card0.Life != 0)
							{
								ProcessTriggers(victoriousTriggers, card0.uid);
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
							Card? c = player.field.GetByPosition(i);
							if(c != null)
							{
								if(c.Keywords.ContainsKey(Keyword.Brittle))
								{
									DestroyImpl(c);
								}
								if(c.Keywords.ContainsKey(Keyword.Decaying))
								{
									RegisterTemporaryLingeringEffectImpl(info: new LingeringEffectInfo(effect: (_) => c.Life -= 1, referrer: c));
									EvaluateLingeringEffects();
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
					turnPlayer = 1 - turnPlayer;
					turn++;
					if(GameConstants.MOMENTUM_INCREMENT_TURNS.Contains(turn))
					{
						momentumBase++;
					}
					ProcessStateReachedTriggers();
					state = GameConstants.State.TurnStart;
				}
				break;
				default:
					throw new NotImplementedException(state.ToString());
			}
			SendFieldUpdates();
		}
		return false;
	}

	private void ProcessStateReachedTriggers()
	{
		// TODO: If this is slow, index by state?
		if(stateReachedTriggers.Count > 0)
		{
			foreach(Player player in players)
			{
				if(stateReachedTriggers.ContainsKey(player.quest.uid))
				{
					foreach(StateReachedTrigger trigger in stateReachedTriggers[player.quest.uid])
					{
						if(trigger.state == state && trigger.condition())
						{
							trigger.effect();
							trigger.wasTriggered = true;
							if(!rewardClaimed && player.quest.Progress >= player.quest.Goal)
							{
								player.quest.Reward();
								player.quest.Text += "\nREWARD CLAIMED";
								rewardClaimed = true;
							}
						}
					}
					stateReachedTriggers[player.quest.uid].RemoveAll(x => x.oneshot && x.wasTriggered);
				}

				foreach(Card card in player.hand.GetAll())
				{
					if(stateReachedTriggers.ContainsKey(card.uid))
					{
						foreach(StateReachedTrigger trigger in stateReachedTriggers[card.uid])
						{
							if(trigger.state == state && trigger.influenceLocation.HasFlag(GameConstants.Location.Hand) && trigger.condition())
							{
								trigger.effect();
								trigger.wasTriggered = true;
							}
							else
							{
								trigger.wasTriggered = false;
							}
						}
						stateReachedTriggers[card.uid].RemoveAll(x => x.oneshot && x.wasTriggered);
					}
				}
				foreach(Card? card in player.field.GetAll())
				{
					if(card != null && stateReachedTriggers.ContainsKey(card.uid))
					{
						foreach(StateReachedTrigger trigger in stateReachedTriggers[card.uid])
						{
							if(trigger.state == state && trigger.influenceLocation.HasFlag(GameConstants.Location.Field) && trigger.condition())
							{
								trigger.effect();
								trigger.wasTriggered = true;
							}
							else
							{
								trigger.wasTriggered = false;
							}
						}
						stateReachedTriggers[card.uid].RemoveAll(x => x.oneshot && x.wasTriggered);
					}
				}
			}
		}
	}

	private void CheckIfLost(int player)
	{
		if(players[player].life <= 0)
		{
			SendFieldUpdates();
			SendPacketToPlayer<DuelPackets.GameResultResponse>(new DuelPackets.GameResultResponse
			{
				result = GameConstants.GameResult.Lost
			}, player);
			SendPacketToPlayer<DuelPackets.GameResultResponse>(new DuelPackets.GameResultResponse
			{
				result = GameConstants.GameResult.Won
			}, 1 - player);
		}
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
		for(int i = 0; i < damage; i++)
		{
			Card c = players[player].deck.GetAt(0);
			players[player].deck.MoveToBottom(0);
			Card?[] shownCards = new Card[2];
			shownCards[player] = c;
			SendFieldUpdates(shownCards: shownCards);
			if(revelationTriggers.ContainsKey(c.uid))
			{
				foreach(RevelationTrigger trigger in revelationTriggers[c.uid])
				{
					if(trigger.condition())
					{
						trigger.effect();
					}
				}
			}
			SendFieldUpdates();
		}
		players[player].deck.Shuffle();
	}
	private void MarkNextZoneOrContinue()
	{
		if(markedZone == null)
		{
			return;
		}
		if(turnPlayer == 0)
		{
			markedZone++;
			if(markedZone == GameConstants.FIELD_SIZE)
			{
				markedZone = null;
				state = GameConstants.State.TurnEnd;
				return;
			}
		}
		else
		{
			markedZone--;
			if(markedZone < 0)
			{
				markedZone = null;
				state = GameConstants.State.TurnEnd;
				return;
			}
		}
		initPlayer = turnPlayer;
		state = GameConstants.State.BattleInitGained;
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
			if(playerStreams[i].DataAvailable)
			{
				List<byte> bytes = ReceiveRawPacket(playerStreams[i])!;
				if(bytes.Count == 0)
				{
					Log("Request was empty, ignoring it", severity: LogSeverity.Warning);
				}
				else
				{
					Program.replay?.actions.Add(new Replay.GameAction(packet: bytes, player: i, clientToServer: true));
					byte type = bytes[0];
					string packet = Encoding.UTF8.GetString(bytes.GetRange(1, bytes.Count - 1).ToArray());
					if(HandlePacket(type, packet, i))
					{
						Log($"{players[i].name} is giving up, closing.");
						return true;
					}
				}
			}
			else
			{
				Thread.Sleep(100);
			}
		}
		return false;
	}

	private bool HandlePacket(byte typeByte, string packet, int player)
	{
		// THIS MIGHT CHANGE AS SENDING RAW JSON MIGHT BE TOO EXPENSIVE/SLOW
		// possible improvements: Huffman or Burrows-Wheeler+RLE
		if(typeByte >= (byte)NetworkingConstants.PacketType.PACKET_COUNT)
		{
			throw new Exception($"ERROR: Unknown packet type encountered: ({typeByte})");
		}
		NetworkingConstants.PacketType type = (NetworkingConstants.PacketType)typeByte;

		List<byte> payload = new List<byte>();
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
				if(!GetCardActions(player, request.uid, request.location).Contains(request.desc))
				{
					Log("Tried to use an option that is not present for that card");
				}
				else
				{
					TakeAction(player, request.uid, request.location, request.desc!);
				}
				state &= ~GameConstants.State.InitGained;
				state |= GameConstants.State.ActionTaken;
			}
			break;
			case NetworkingConstants.PacketType.DuelPassRequest:
			{
				switch(state)
				{
					case GameConstants.State.MainInitGained:
					{
						if(players[1 - player].passed)
						{
							state = GameConstants.State.BattleStart;
						}
						else
						{
							players[player].passed = true;
							state = GameConstants.State.MainActionTaken;
						}
					}
					break;
					case GameConstants.State.BattleInitGained:
					{
						if(players[1 - player].passed)
						{
							state = GameConstants.State.DamageCalc;
						}
						else
						{
							players[player].passed = true;
							state = GameConstants.State.BattleActionTaken;
						}
					}
					break;
					default:
						Log($"Unable to pass in state {state}", severity: LogSeverity.Warning);
						break;
				}
			}
			break;
			case NetworkingConstants.PacketType.DuelViewGraveRequest:
			{
				bool opponent = DeserializeJson<DuelPackets.ViewGraveRequest>(packet).opponent;
				SendPacketToPlayer<DuelPackets.ViewCardsResponse>(new DuelPackets.ViewCardsResponse
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
		if(activatedEffects.ContainsKey(uid))
		{
			foreach(ActivatedEffectInfo info in activatedEffects[uid])
			{
				if(info.influenceLocation.HasFlag(location) && option == info.name)
				{
					info.effect();
					info.uses++;
					SendFieldUpdates();
					EvaluateLingeringEffects();
					return;
				}
			}
		}
		switch(location)
		{
			case GameConstants.Location.Hand:
			{
				Card card = players[player].hand.GetByUID(uid);
				if(option == "Cast")
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
				if(players[player].quest.Position >= players[player].quest.Cost)
				{
					throw new NotImplementedException($"GetActions for ignition quests");
				}
			}
			break;
			case GameConstants.Location.Ability:
			{
				if(players[player].ability.Position == 0 && players[player].momentum > 0 && castTriggers.ContainsKey(players[player].ability.uid))
				{
					players[player].momentum--;
					foreach(CastTrigger trigger in castTriggers[players[player].ability.uid])
					{
						if(trigger.condition())
						{
							trigger.effect();
						}
					}
					// TODO: Tidy this up and deduplicate with CastImpl
					foreach(Player p in players)
					{
						if(genericCastTriggers.ContainsKey(p.quest.uid))
						{
							foreach(GenericCastTrigger trigger in genericCastTriggers[p.quest.uid])
							{
								if(trigger.condition(castCard: players[player].ability))
								{
									trigger.effect(castCard: players[player].ability);
									if(!rewardClaimed && p.quest.Progress >= p.quest.Goal)
									{
										p.quest.Reward();
										p.quest.Text += "\nREWARD CLAIMED";
										rewardClaimed = true;
										break;
									}
								}
							}
						}
						foreach(Card possiblyTriggeringCard in p.field.GetUsed())
						{
							if(genericCastTriggers.ContainsKey(possiblyTriggeringCard.uid))
							{
								foreach(GenericCastTrigger trigger in genericCastTriggers[possiblyTriggeringCard.uid])
								{
									if(trigger.influenceLocation.HasFlag(GameConstants.Location.Field) && trigger.condition(castCard: players[player].ability))
									{
										trigger.effect(castCard: players[player].ability);
									}
								}
							}
						}
						players[player].ability.Position = 1;
					}
				}
			}
			break;
			case GameConstants.Location.Field:
			{
				Card card = players[player].field.GetByUID(uid);
				if(option == "Move")
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

	public void MoveImpl(Card card, int zone)
	{
		players[card.Controller].field.Move(card.Position, zone);
		SendFieldUpdates();
	}

	private string[] GetCardActions(int player, int uid, GameConstants.Location location)
	{
		if(player != initPlayer)
		{
			return new string[0];
		}
		List<string> options = new List<string>();
		if(activatedEffects.ContainsKey(uid))
		{
			foreach(ActivatedEffectInfo info in activatedEffects[uid])
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
					!(state.HasFlag(GameConstants.State.BattleStart) && card.CardType == GameConstants.CardType.Creature))
				{
					bool canCast = true;
					if(castTriggers.ContainsKey(card.uid))
					{
						foreach(CastTrigger trigger in castTriggers[card.uid])
						{
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
						options.Add("Cast");
					}
				}
			}
			break;
			case GameConstants.Location.Ability:
			{
				if(players[player].ability.Position == 0 && players[player].momentum > 0 && castTriggers.ContainsKey(players[player].ability.uid))
				{
					options.Add("Use");
				}
			}
			break;
			case GameConstants.Location.Field:
			{
				Card card = players[player].field.GetByUID(uid);
				if(players[player].field.CanMove(card.Position, players[player].momentum))
				{
					options.Add("Move");
				}
			}
			break;
			case GameConstants.Location.Quest:
				Log("Quests are not foreseen to have activated abilities", severity: LogSeverity.Warning);
				break;
			default:
				throw new NotImplementedException($"GetCardActions at {location}");
		}
		return options.ToArray();
	}

	public bool AskYesNoImpl(int player, string question)
	{
		Log("Asking yes no");
		SendPacketToPlayer(new DuelPackets.YesNoRequest { question = question }, player);
		Log("Receiving");
		return ReceivePacketFromPlayer<DuelPackets.YesNoResponse>(player).result;
	}
	private void SendFieldUpdates(GameConstants.Location mask = GameConstants.Location.ALL, Card?[]? shownCards = null)
	{
		for(int i = 0; i < players.Length; i++)
		{
			SendFieldUpdate(i, mask, ownShownCard: shownCards?[i], oppShownCard: shownCards?[1 - i]);
		}
	}
	private void SendFieldUpdate(int player, GameConstants.Location mask, Card? ownShownCard, Card? oppShownCard)
	{
		// TODO: actually handle mask if this is too slow
		DuelPackets.FieldUpdateRequest request = new DuelPackets.FieldUpdateRequest()
		{
			turn = turn + 1,
			hasInitiative = state != GameConstants.State.UNINITIALIZED && initPlayer == player,
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
				shownCard = ownShownCard?.ToStruct(),
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
				shownCard = oppShownCard?.ToStruct(),
			},
		};
		SendPacketToPlayer<DuelPackets.FieldUpdateRequest>(request, player);
	}
	public Card[] SelectCardsCustom(int player, string description, Card[] cards, Func<Card[], bool> isValidSelection)
	{
		Log("Select cards custom");
		SendPacketToPlayer(new DuelPackets.CustomSelectCardsRequest
		{
			cards = Card.ToStruct(cards),
			desc = description,
			initialState = isValidSelection(new Card[0])
		}, player);

		Log("request sent");
		List<byte> payload = new List<byte>();
		do
		{
			DuelPackets.CustomSelectCardsIntermediateRequest request;
			payload = ReceiveRawPacket(playerStreams[player])!;
			Log("request received");
			if(payload[0] == (byte)NetworkingConstants.PacketType.DuelCustomSelectCardsResponse)
			{
				Log("breaking out");
				break;
			}
			if(payload[0] != (byte)NetworkingConstants.PacketType.DuelCustomSelectCardsIntermediateRequest)
			{
				continue;
			}
			Program.replay?.actions.Add(new Replay.GameAction(player: player, packet: payload, clientToServer: true));
			request = DeserializePayload<DuelPackets.CustomSelectCardsIntermediateRequest>(payload);
			Log("deserialized packet");
			SendPacketToPlayer(new DuelPackets.CustomSelectCardsIntermediateResponse
			{
				isValid = isValidSelection(Array.ConvertAll(request.uids, (x => cards.First(y => y.uid == x))))
			}, player);
			Log("sent packet");
		} while(true);

		DuelPackets.CustomSelectCardsResponse response = DeserializePayload<DuelPackets.CustomSelectCardsResponse>(payload);
		Log("final response");
		Card[] ret = cards.Where(x => response.uids.Contains(x.uid)).ToArray();
		if(!isValidSelection(ret))
		{
			throw new Exception("Player somethow selected invalid cards");
		}
		Log("returning");
		return ret;
	}
	private int SelectMovementZone(int player, int position, int momentum)
	{
		SendPacketToPlayer<DuelPackets.SelectZoneRequest>(new DuelPackets.SelectZoneRequest
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
	private void MoveToFieldImpl(int choosingPlayer, int targetPlayer, Card card)
	{
		RemoveCardFromItsLocation(card);
		int zone = SelectZoneImpl(choosingPlayer: choosingPlayer, targetPlayer: targetPlayer);
		card.Controller = targetPlayer;
		players[targetPlayer].field.Add(card, zone);
	}
	private void CastImpl(int player, Card card)
	{
		if(!card.isInitialized)
		{
			card.Init();
			card.isInitialized = true;
		}
		RemoveCardFromItsLocation(card);
		Card?[] shownCards = new Card[2];
		shownCards[player] = card;
		SendFieldUpdates(shownCards: shownCards);
		switch(card.CardType)
		{
			case GameConstants.CardType.Creature:
			{
				players[player].CastCreature(card, SelectZoneImpl(player, player));
			}
			break;
			case GameConstants.CardType.Spell:
			{
				players[player].CastSpell(card);
			}
			break;
			default:
				throw new NotImplementedException($"Casting {card.CardType} cards");
		}
		if(!players[player].castCounts.ContainsKey(card.Name))
		{
			players[player].castCounts[card.Name] = 0;
		}
		players[player].castCounts[card.Name]++;
		ProcessTriggers(castTriggers, card.uid);
		foreach(Player p in players)
		{
			if(genericCastTriggers.ContainsKey(p.quest.uid))
			{
				foreach(GenericCastTrigger trigger in genericCastTriggers[p.quest.uid])
				{
					if(trigger.condition(castCard: card))
					{
						trigger.effect(castCard: card);
						if(!rewardClaimed && p.quest.Progress >= p.quest.Goal)
						{
							p.quest.Reward();
							p.quest.Text += "\nREWARD CLAIMED";
							rewardClaimed = true;
							break;
						}
					}
				}
			}
			foreach(Card possiblyTriggeringCard in p.field.GetUsed())
			{
				if(genericCastTriggers.ContainsKey(possiblyTriggeringCard.uid))
				{
					foreach(GenericCastTrigger trigger in genericCastTriggers[possiblyTriggeringCard.uid])
					{
						if(trigger.influenceLocation.HasFlag(GameConstants.Location.Field) && trigger.condition(castCard: card))
						{
							trigger.effect(castCard: card);
						}
					}
				}
			}
		}
		EvaluateLingeringEffects();
		SendFieldUpdates();
	}

	public void ResetAbilityImpl(int player)
	{
		players[player].ability.Position = 0;
	}

	public void RegisterCastTriggerImpl(CastTrigger trigger, Card referrer)
	{
		if(!castTriggers.ContainsKey(referrer.uid))
		{
			castTriggers[referrer.uid] = new List<CastTrigger>();
		}
		castTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterDealsDamageTriggerImpl(Trigger trigger, Card referrer)
	{
		if(!dealsDamageTriggers.ContainsKey(referrer.uid))
		{
			dealsDamageTriggers[referrer.uid] = new List<Trigger>();
		}
		dealsDamageTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterGenericCastTriggerImpl(GenericCastTrigger trigger, Card referrer)
	{
		if(!genericCastTriggers.ContainsKey(referrer.uid))
		{
			genericCastTriggers[referrer.uid] = new List<GenericCastTrigger>();
		}
		genericCastTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterRevelationTriggerImpl(RevelationTrigger trigger, Card referrer)
	{
		if(!revelationTriggers.ContainsKey(referrer.uid))
		{
			revelationTriggers[referrer.uid] = new List<RevelationTrigger>();
		}
		revelationTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterYouDiscardTriggerImpl(DiscardTrigger trigger, Card referrer)
	{
		if(!youDiscardTriggers.ContainsKey(referrer.uid))
		{
			youDiscardTriggers[referrer.uid] = new List<DiscardTrigger>();
		}
		youDiscardTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterDiscardTriggerImpl(DiscardTrigger trigger, Card referrer)
	{
		if(!discardTriggers.ContainsKey(referrer.uid))
		{
			discardTriggers[referrer.uid] = new List<DiscardTrigger>();
		}
		discardTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterStateReachedTriggerImpl(StateReachedTrigger trigger, Card referrer)
	{
		if(!stateReachedTriggers.ContainsKey(referrer.uid))
		{
			stateReachedTriggers[referrer.uid] = new List<StateReachedTrigger>();
		}
		stateReachedTriggers[referrer.uid].Add(trigger);
	}
	public void RegisterLingeringEffectImpl(LingeringEffectInfo info)
	{
		if(info.influenceLocation == GameConstants.Location.ALL)
		{
			alwaysActiveLingeringEffects.Add(info);
		}
		else
		{
			if(!lingeringEffects.ContainsKey(info.referrer.uid))
			{
				lingeringEffects[info.referrer.uid] = new List<LingeringEffectInfo>();
			}
			lingeringEffects[info.referrer.uid].Add(info);
		}
	}
	public void RegisterTemporaryLingeringEffectImpl(LingeringEffectInfo info)
	{
		if(!temporaryLingeringEffects.ContainsKey(info.referrer.uid))
		{
			temporaryLingeringEffects[info.referrer.uid] = new List<LingeringEffectInfo>();
		}
		temporaryLingeringEffects[info.referrer.uid].Add(info);
	}
	public void RegisterActivatedEffectImpl(ActivatedEffectInfo info)
	{
		if(!activatedEffects.ContainsKey(info.referrer.uid))
		{
			activatedEffects[info.referrer.uid] = new List<ActivatedEffectInfo>();
		}
		activatedEffects[info.referrer.uid].Add(info);
	}
	public void RegisterVictoriousTriggerImpl(Trigger info, Card referrer)
	{
		if(!victoriousTriggers.ContainsKey(referrer.uid))
		{
			victoriousTriggers[referrer.uid] = new List<Trigger>();
		}
		victoriousTriggers[referrer.uid].Add(info);
	}
	public void RegisterAttackTriggerImpl(Trigger info, Card referrer)
	{
		if(!attackTriggers.ContainsKey(referrer.uid))
		{
			attackTriggers[referrer.uid] = new List<Trigger>();
		}
		attackTriggers[referrer.uid].Add(info);
	}
	public void RegisterDeathTriggerImpl(Trigger info, Card referrer)
	{
		if(!deathTriggers.ContainsKey(referrer.uid))
		{
			deathTriggers[referrer.uid] = new List<Trigger>();
		}
		deathTriggers[referrer.uid].Add(info);
	}
	public void RegisterGenericDeathTriggerImpl(GenericDeathTrigger info, Card referrer)
	{
		if(!genericDeathTriggers.ContainsKey(referrer.uid))
		{
			genericDeathTriggers[referrer.uid] = new List<GenericDeathTrigger>();
		}
		genericDeathTriggers[referrer.uid].Add(info);
	}
	public Card?[] GetFieldImpl(int player)
	{
		return players[player].field.GetAll();
	}
	public Card[] GetFieldUsedImpl(int player)
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
		Card target = SelectCardsImpl(player: player, cards: possibleCards, amount: 1, description: "Select card to gather")[0];
		players[player].deck.Remove(target);
		players[player].hand.Add(target);
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
				players[card.Controller].field.Remove(card);
				break;
			case GameConstants.Location.Grave:
				players[card.Controller].grave.Remove(card);
				break;
			default:
				throw new Exception($"Cannot add a card from {card.Location} to hand");
		}
		players[player].hand.Add(card);
	}
	public Card[] GetDiscardableImpl(int player, Card? ignore)
	{
		return players[player].hand.GetDiscardable(ignore);
	}
	public void DestroyImpl(Card card)
	{
		players[card.Controller].Destroy(card);
		if(temporaryLingeringEffects.Remove(card.uid))
		{
			EvaluateLingeringEffects();
		}
		if(card.Keywords.ContainsKey(Keyword.Brittle))
		{
			players[card.Controller].brittleDeathCounts[turn]++;
		}
		players[card.Controller].deathCounts[turn]++;
		ProcessTriggers(deathTriggers, card.uid);
		SendFieldUpdates();
		foreach(Player player in players)
		{
			foreach(Card fieldCard in player.field.GetUsed())
			{
				if(genericDeathTriggers.ContainsKey(fieldCard.uid))
				{
					foreach(GenericDeathTrigger trigger in genericDeathTriggers[fieldCard.uid])
					{
						if(trigger.influenceLocation.HasFlag(GameConstants.Location.Field) && trigger.condition(destroyedCard: card))
						{
							trigger.effect(destroyedCard: card);
						}
					}
				}
			}
			foreach(Card graveCard in player.grave.GetAll())
			{
				if(genericDeathTriggers.ContainsKey(graveCard.uid))
				{
					foreach(GenericDeathTrigger trigger in genericDeathTriggers[graveCard.uid])
					{
						if(trigger.influenceLocation.HasFlag(GameConstants.Location.Grave) && trigger.condition(destroyedCard: card))
						{
							trigger.effect(destroyedCard: card);
						}
					}
				}
			}
			foreach(Card handCard in player.hand.GetAll())
			{
				if(genericDeathTriggers.ContainsKey(handCard.uid))
				{
					foreach(GenericDeathTrigger trigger in genericDeathTriggers[handCard.uid])
					{
						if(trigger.influenceLocation.HasFlag(GameConstants.Location.Hand) && trigger.condition(destroyedCard: card))
						{
							trigger.effect(destroyedCard: card);
						}
					}
				}
			}
		}
		foreach(Player player in players)
		{
			if(genericDeathTriggers.ContainsKey(player.quest.uid))
			{
				foreach(GenericDeathTrigger trigger in genericDeathTriggers[player.quest.uid])
				{
					if(trigger.condition(destroyedCard: card))
					{
						trigger.effect(destroyedCard: card);
						if(!rewardClaimed && player.quest.Progress >= player.quest.Goal)
						{
							player.quest.Reward();
							player.quest.Text += "\nREWARD CLAIMED";
							rewardClaimed = true;
							break;
						}
					}
				}
			}
		}
		EvaluateLingeringEffects();
	}
	public bool RemoveCardFromItsLocation(Card card)
	{
		switch(card.Location)
		{
			case GameConstants.Location.Hand:
				players[card.Controller].hand.Remove(card);
				break;
			case GameConstants.Location.Field:
				players[card.Controller].field.Remove(card);
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
		bool[] shouldShuffle = new bool[players.Length];
		foreach(Card card in cards)
		{
			if(RemoveCardFromItsLocation(card))
			{
				shouldShuffle[card.Controller] = true;
				players[card.Controller].deck.Add(card);
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
		return response.uids.ToList().ConvertAll(x => cards.First(y => y.uid == x)).ToArray();
	}
	public void DiscardAmountImpl(int player, int amount)
	{
		Card[] targets = SelectCardsImpl(player: player, amount: amount, cards: players[player].hand.GetDiscardable(null), description: "Select cards to discard");
		foreach(Card target in targets)
		{
			DiscardImpl(target);
		}
	}
	public void DiscardImpl(Card card)
	{
		if(card.Location != GameConstants.Location.Hand)
		{
			throw new Exception($"Tried to discard a card that is not in the hand but at {card.Location}");
		}
		players[card.Controller].Discard(card);
		players[card.Controller].discardCounts[turn]++;
		if(discardTriggers.ContainsKey(card.uid))
		{
			foreach(DiscardTrigger trigger in discardTriggers[card.uid])
			{
				if(trigger.condition())
				{
					trigger.effect();
				}
			}
		}
		Player player = players[card.Controller];
		if(youDiscardTriggers.ContainsKey(player.quest.uid))
		{
			foreach(DiscardTrigger trigger in new List<Trigger>(youDiscardTriggers[player.quest.uid]))
			{
				if(trigger.condition())
				{
					trigger.effect();
					if(!rewardClaimed && player.quest.Progress >= player.quest.Goal)
					{
						player.quest.Reward();
						player.quest.Text += "\nREWARD CLAIMED";
						rewardClaimed = true;
					}
				}
			}
		}
		foreach(Card c in player.hand.GetAll())
		{
			if(youDiscardTriggers.ContainsKey(c.uid))
			{
				foreach(DiscardTrigger trigger in youDiscardTriggers[c.uid])
				{
					if(trigger.influenceLocation.HasFlag(GameConstants.Location.Hand) && trigger.condition())
					{
						trigger.effect();
					}
				}
			}
		}
		foreach(Card? c in player.field.GetAll())
		{
			if(c != null && youDiscardTriggers.ContainsKey(c.uid))
			{
				foreach(DiscardTrigger trigger in youDiscardTriggers[c.uid])
				{
					if(trigger.influenceLocation.HasFlag(GameConstants.Location.Field) && trigger.condition())
					{
						trigger.effect();
					}
				}
			}
		}
	}

	public Card CreateTokenImpl(int player, int power, int life, string name)
	{
		if(!players[player].field.HasEmpty())
		{
			throw new Exception($"Tried to create a token but the field is full");
		}
		int zone = SelectZoneImpl(player, player);
		Token token = new Token
		(
			Name: name,
			Text: "[Token]",
			OriginalCost: 0,
			OriginalLife: life,
			OriginalPower: power,
			Controller: player
		);
		players[player].field.Add(token, zone);
		return token;
	}
	public Card CreateTokenCopyImpl(int player, Card card)
	{
		if(!players[player].field.HasEmpty())
		{
			throw new Exception($"Tried to create a token but the field is full");
		}
		int zone = SelectZoneImpl(choosingPlayer: player, targetPlayer: player);
		Card token;
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
			token = CreateBasicCard(card.GetType(), player);
			token.RegisterKeyword(Keyword.Token);
		}
		players[player].field.Add(token, zone);
		return token;
	}
	public int SelectZoneImpl(int choosingPlayer, int targetPlayer)
	{
		bool[] options = players[targetPlayer].field.GetPlacementOptions();
		if(choosingPlayer != targetPlayer)
		{
			options = options.Reverse().ToArray();
		}
		SendPacketToPlayer<DuelPackets.SelectZoneRequest>(new DuelPackets.SelectZoneRequest
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
		List<byte> payload = ReceivePacket<T>(playerStreams[player])!;
		Program.replay?.actions.Add(new Replay.GameAction(player: player, packet: payload, clientToServer: true));
		return DeserializePayload<T>(payload);
	}
	private static Stopwatch watch = new Stopwatch();
	public static void SendPacketToPlayer<T>(T packet, int player) where T : PacketContent
	{
		List<byte> payload = Functions.GeneratePayload<T>(packet);
		Program.replay?.actions.Add(new Replay.GameAction(player: player, packet: payload.GetRange(0, payload.Count - NetworkingStructs.Packet.ENDING.Length), clientToServer: false));
#if(DEBUG)
		if(watch.IsRunning)
		{
			Log($"Elapsed since last packet: {watch.Elapsed}");
			watch.Restart();
		}
		else
		{
			watch.Restart();
		}
#endif
		playerStreams[player].Write(payload.ToArray(), 0, payload.Count);
	}
}