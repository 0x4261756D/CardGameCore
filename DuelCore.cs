using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using CardGameUtils;
using CardGameUtils.Structs;
using static CardGameUtils.Functions;
using static CardGameUtils.Structs.NetworkingStructs;

namespace CardGameCore;

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
	private int count;
	private SHA384 sha;
	public Player[] players;
	public static NetworkStream[] playerStreams = new NetworkStream[0];
	public static Random rnd = new Random();
	public const int HASH_LEN = 96;
	public int playersConnected = 0;
	public int turn, turnPlayer, initPlayer;
	public int? markedZone = null;
	public int momentumBase = GameConstants.START_MOMENTUM;

	private Dictionary<int, List<CastTrigger>> castTriggers = new Dictionary<int, List<CastTrigger>>();
	private Dictionary<int, List<RevelationTrigger>> revelationTriggers = new Dictionary<int, List<RevelationTrigger>>();
	private Dictionary<int, List<StateReachedTrigger>> stateReachedTriggers = new Dictionary<int, List<StateReachedTrigger>>();
	private Dictionary<int, List<LingeringEffectInfo>> lingeringEffects = new Dictionary<int, List<LingeringEffectInfo>>();

	public DuelCore()
	{
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
			Card quest = CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[2].Substring(1))!, i);
			for(int j = 3; j < Program.config.duel_config.players[i].decklist.Length; j++)
			{
				deck.Add(CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[j])!, i));
			}
			players[i] = new Player(Program.config.duel_config.players[i], i, deck, playerClass, ability, quest);
		}
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
		c.uid = count;
		count++;
		c.Controller = controller;
		c.RegisterCastTrigger = RegisterCastTriggerImpl;
		c.RegisterStateReachedTrigger = RegisterStateReachedTriggerImpl;
		c.RegisterLingeringEffect = RegisterLingeringEffectImpl;
		c.GetField = GetFieldImpl;
		c.GetHand = GetHandImpl;
		c.SelectCards = SelectCardsImpl;
		c.Discard = DiscardImpl;
		c.CreateToken = CreateTokenImpl;
		c.GetDiscardCountThisTurn = GetDiscardCountThisTurnImpl;
		c.Init();
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
					break;
				}
				if(HandlePlayerActions())
				{
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
		foreach(Player player in players)
		{
			foreach(Card? card in player.field.GetAll())
			{
				if(card != null && lingeringEffects.ContainsKey(card.uid))
				{
					foreach(LingeringEffectInfo info in lingeringEffects[card.uid])
					{
						if(info.influenceLocation.HasFlag(card.Location))
						{
							info.effect();
						}
					}
				}
			}
		}
	}

	private bool HandleGameLogic()
	{
		while(!state.HasFlag(GameConstants.State.InitGained))
		{
			EvaluateLingeringEffects();
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
						player.progress = 0;
					}
					SendFieldUpdates();
					// Mulligan
					for(int i = 0; i < players.Length; i++)
					{
						if(AskYesNo(player: i, question: "Mulligan?"))
						{
							Card[] cards = SelectCardsCustom(i, "Select cards to mulligan", players[i].hand.GetAll(), (x) => true);
							foreach(Card card in cards)
							{
								players[i].hand.Remove(card);
								players[i].deck.Add(card);
							}
							SendFieldUpdates();
							players[i].deck.Shuffle();
							players[i].Draw(cards.Length);
							SendFieldUpdates();
						}
					}
					turnPlayer = rnd.Next(100) / 50;
					turn = 0;
					state = GameConstants.State.TurnStart;
				}
				break;
				case GameConstants.State.TurnStart:
				{
					foreach(Player player in players)
					{
						player.Draw(1);
						player.ability.Position = 0;
						player.discardCountThisTurn = 0;
						player.momentum = momentumBase;
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
					if(card0 == null)
					{
						if(card1 != null)
						{
							// Deal damage to player
							DealDamage(0, card1.Power);
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
							DealDamage(1, card0.Power);
							if(players[1].life <= 0)
							{
								return true;
							}
						}
						else
						{
							card0.BaseLife -= card1.BasePower;
							card1.BaseLife -= card0.BasePower;
							EvaluateLingeringEffects();
							if(card0.Power == 0)
							{
								players[0].Destroy(card0);
							}
							if(card1.Power == 0)
							{
								players[1].Destroy(card1);
							}
						}
					}
					foreach (Player player in players)
					{
						player.passed = false;
					}
					MarkNextZoneOrContinue();
				}
				break;
				case GameConstants.State.TurnEnd:
				{
					turn++;
					turnPlayer = 1 - turnPlayer;
					if(GameConstants.MOMENTUM_INCREMENT_TURNS.Contains(turn))
					{
						momentumBase++;
					}
					ProcessStateReachedTriggers();
					foreach (Player player in players)
					{
						for(int i = 0; i < GameConstants.FIELD_SIZE; i++)
						{
							Card? c = player.field.GetByPosition(i);
							if(c != null && c.Keywords.ContainsKey(Keyword.Brittle))
							{
								player.field.Remove(c);
							}
						}
					}
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
				foreach(Card card in player.hand.GetAll())
				{
					if(stateReachedTriggers.ContainsKey(card.uid))
					{
						foreach(StateReachedTrigger trigger in stateReachedTriggers[card.uid])
						{
							if(trigger.state == state && trigger.influenceLocation.HasFlag(GameConstants.Location.Hand) && trigger.condition())
							{
								trigger.effect();
							}
						}
					}
				}
				foreach(Card? card in player.field.GetAll())
				{
					if(card != null && stateReachedTriggers.ContainsKey(card.uid))
					{
						foreach(StateReachedTrigger trigger in stateReachedTriggers[card.uid])
						{
							if(trigger.state == state && trigger.influenceLocation.HasFlag(GameConstants.Location.Hand) && trigger.condition())
							{
								trigger.effect();
							}
						}
					}
				}
			}
		}
	}

	{
		players[player].life -= damage;
		Reveal(player, damage);
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
	private void Reveal(int player, int damage)
	{
		for(int i = 0; i < damage; i++)
		{
			Card c = players[player].deck.Pop();
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
			players[player].grave.Add(c);
			SendFieldUpdates();
		}
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
					byte type = bytes[0];
					bytes.RemoveAt(0);
					string packet = Encoding.UTF8.GetString(bytes.ToArray());
					if(HandlePacket(type, packet, i))
					{
						Log($"{players[i].name} is giving up, closing.");
						return true;
					}
				}
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
						throw new Exception($"Unable to pass in state {state}");
				}
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
		switch(location)
		{
			case GameConstants.Location.Hand:
			{
				Card card = players[player].hand.GetByUID(uid);
				if(option == "Cast")
				{
					Cast(player, card);
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
				if(players[player].ability.Position == 0 && castTriggers.ContainsKey(players[player].ability.uid))
				{
					foreach(CastTrigger trigger in castTriggers[players[player].ability.uid])
					{
						if(trigger.condition())
						{
							trigger.effect();
						}
					}
					players[player].ability.Position = 1;
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
						players[player].field.Move(card.Position, zone);
						SendFieldUpdates();
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

	private string[] GetCardActions(int player, int uid, GameConstants.Location location)
	{
		if(player != initPlayer)
		{
			return new string[0];
		}
		List<string> options = new List<string>();
		switch(location)
		{
			case GameConstants.Location.Hand:
			{
				Card card = players[player].hand.GetByUID(uid);
				if(card.Cost <= players[player].momentum &&
					!(state.HasFlag(GameConstants.State.BattleStart) && card.CardType == GameConstants.CardType.Creature))
				{
					options.Add("Cast");
				}
			}
			break;
			case GameConstants.Location.Quest:
			{
				if(players[player].quest.Position >= players[player].quest.Cost)
				{
					options.Add("Activate");
				}
			}
			break;
			case GameConstants.Location.Ability:
			{
				if(players[player].ability.Position == 0 && castTriggers.ContainsKey(players[player].ability.uid))
				{
					options.Add("Activate");
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
			default:
				throw new NotImplementedException($"GetCardActions at {location}");
		}
		return options.ToArray();
	}

	public bool AskYesNo(int player, string question)
	{
		SendPacketToPlayer(new DuelPackets.YesNoRequest { question = question }, player);
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
		SendPacketToPlayer(new DuelPackets.CustomSelectCardsRequest
		{
			cards = Card.ToStruct(cards),
			desc = description,
			initialState = isValidSelection(new Card[0])
		}, player);

		List<byte> payload = new List<byte>();
		do
		{
			DuelPackets.CustomSelectCardsIntermediateRequest request;
			payload = ReceiveRawPacket(playerStreams[player])!;
			if(payload[0] == (byte)NetworkingConstants.PacketType.DuelCustomSelectCardsResponse)
			{
				break;
			}
			request = DeserializePayload<DuelPackets.CustomSelectCardsIntermediateRequest>(payload);
			SendPacketToPlayer(new DuelPackets.CustomSelectCardsIntermediateResponse
			{
				isValid = isValidSelection(Array.ConvertAll(request.uids, (x => cards.First(y => y.uid == x))))
			}, player);
		} while(true);

		DuelPackets.CustomSelectCardsResponse response = DeserializePayload<DuelPackets.CustomSelectCardsResponse>(payload);
		Card[] ret = cards.Where(x => response.uids.Contains(x.uid)).ToArray();
		if(!isValidSelection(ret))
		{
			throw new Exception("Player somethow selected invalid cards");
		}
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
	private void Cast(int player, Card card)
	{
		switch(card.CardType)
		{
			case GameConstants.CardType.Creature:
			{
				players[player].CastCreature(card, SelectZoneImpl(player));
			}
			break;
			case GameConstants.CardType.Spell:
			{
				players[player].hand.Remove(card);
			}
			break;
			default:
				throw new NotImplementedException($"Casting {card.CardType} cards");
		}
		if(castTriggers.ContainsKey(card.uid))
		{
			EffectChain chain = new EffectChain(players.Length);
			foreach(Trigger trigger in castTriggers[card.uid])
			{
				Log($"trigger condition met: {trigger.condition()}");
				if(trigger.condition())
				{
					chain.Push(card, trigger.effect);
				}
			}
			Log($"trigger chain length: {chain.Count()}");
			// TODO: Add handling of opponent's responses here
			while(chain.Pop()) { }
		}
		EvaluateLingeringEffects();
		SendFieldUpdates();
	}

	public void RegisterCastTriggerImpl(Effect effect, TriggerCondition condition, Card referrer)
	{
		if(!castTriggers.ContainsKey(referrer.uid))
		{
			castTriggers[referrer.uid] = new List<CastTrigger>();
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
		if(!lingeringEffects.ContainsKey(info.referrer.uid))
		{
			lingeringEffects[info.referrer.uid] = new List<LingeringEffectInfo>();
		}
		lingeringEffects[info.referrer.uid].Add(info);
	}
	public Card?[] GetFieldImpl(int player)
	{
		return players[player].field.GetAll();
	}
	public Card[] GetHandImpl(int player)
	{
		return players[player].hand.GetAll();
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
	public void DiscardImpl(Card card)
	{
		if(card.Location != GameConstants.Location.Hand)
		{
			throw new Exception($"Tried to discard a card that is not in the hand but at {card.Location}");
		}
		players[card.Controller].Discard(card);
	}
	public void CreateTokenImpl(int player, int power, int life, string name)
	{
		if(!players[player].field.HasEmpty())
		{
			throw new Exception($"Tried to create a token but the field is full");
		}
		int zone = SelectZoneImpl(player);
		players[player].field.Add(new Token
		(
			Name: name,
			Text: "[Token]",
			OriginalCost: 0,
			OriginalLife: life,
			OriginalPower: power
		), zone);
	}
	public int SelectZoneImpl(int player)
	{
		SendPacketToPlayer<DuelPackets.SelectZoneRequest>(new DuelPackets.SelectZoneRequest
		{
			options = players[player].field.GetPlacementOptions(),
		}, player);
		return ReceivePacketFromPlayer<DuelPackets.SelectZoneResponse>(player).zone;
	}
	public int GetDiscardCountThisTurnImpl(int player)
	{
		return players[player].discardCountThisTurn;
	}

	public static T ReceivePacketFromPlayer<T>(int player) where T : PacketContent
	{
		T ret = ReceivePacket<T>(playerStreams[player])!;
		return ret;
	}
	public static void SendPacketToPlayer<T>(T packet, int player) where T : PacketContent
	{
		List<byte> payload = Functions.GeneratePayload<T>(packet);
		playerStreams[player].Write(payload.ToArray(), 0, payload.Count);
	}
}