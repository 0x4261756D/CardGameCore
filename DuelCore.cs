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
	public int momentumCount = GameConstants.START_MOMENTUM;
	public DuelCore()
	{
		sha = SHA384.Create();
		players = new Player[Program.config.duel_config!.players.Length];
		playerStreams = new NetworkStream[Program.config.duel_config.players.Length];
		for (int i = 0; i < players.Length; i++)
		{
			Log("Player created. ID: " + Program.config.duel_config.players[i].id);
			Deck deck = new Deck();
			GameConstants.PlayerClass playerClass = Enum.Parse<GameConstants.PlayerClass>(Program.config.duel_config.players[i].decklist[0]);
			if (!Program.config.duel_config.players[i].decklist[1].StartsWith("#"))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no ability, {Program.config.duel_config.players[i].decklist[1]} is no suitable ability");
				return;
			}
			Card ability = CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[1].Substring(1))!, i);
			if (!Program.config.duel_config.players[i].decklist[2].StartsWith("|"))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no quest, {Program.config.duel_config.players[i].decklist[2]} is no suitable ability");
				return;
			}
			Card quest = CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[2].Substring(1))!, i);
			for (int j = 3; j < Program.config.duel_config.players[i].decklist.Length; j++)
			{
				deck.Add(CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[j])!, i));
			}
			players[i] = new Player(Program.config.duel_config.players[i], i, deck, playerClass, ability, quest);
		}
		HandleNetworking();
		foreach (NetworkStream stream in playerStreams)
		{
			if (stream != null)
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
		c.Init();
		return c;
	}

	public override void HandleNetworking()
	{
		listener.Start();
		while (true)
		{
			// Connect the players if they aren't yet
			if (playersConnected < players.Length && listener.Pending())
			{
				NetworkStream stream = listener.AcceptTcpClient().GetStream();
				byte[] buf = new byte[256];
				int len = stream.Read(buf, 0, HASH_LEN);
				if (len != HASH_LEN)
				{
					Log($"len was {len} but expected {HASH_LEN}\n-------------------\n{Encoding.UTF8.GetString(buf)}", severity: LogSeverity.Error);
					//FIXME: Be more nice than simply killing the connection
					stream.Close();
					continue;
				}
				string id = Encoding.UTF8.GetString(buf, 0, len);
				bool foundPlayer = false;
				for (int i = 0; i < players.Length; i++)
				{
					if (playerStreams[i] == null)
					{
						Log($"Player id: {players[i].id} ({players[i].id.Length}), found {id} ({id.Length}) | {players[i].id == id}");
						if (players[i].id == id)
						{
							playersConnected++;
							foundPlayer = true;
							playerStreams[i] = stream;
							stream.WriteByte((byte)i);
						}
					}
				}
				if (!foundPlayer)
				{
					Log("Found no player", severity: LogSeverity.Error);
					//FIXME: Be more nice, see above
					stream.Close();
				}
				DateTime t = DateTime.Now;
				Log($"{t.ToLongTimeString()}:{t.Millisecond} New Player {playersConnected}/{players.Length}");
			}
			if (playersConnected == players.Length)
			{
				if (HandleGameLogic())
				{
					break;
				}
				if (HandlePlayerActions())
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
		if (len != HASH_LEN)
		{
			Log($"len was {len} but expected {HASH_LEN}\n-------------------\n{Encoding.UTF8.GetString(buf)}", severity: LogSeverity.Error);
			//FIXME: Be more nice than simply killing the connection
			stream.Close();
			return;
		}
		string id = Encoding.UTF8.GetString(buf, 0, len);
		bool foundPlayer = false;
		for (int i = 0; i < players.Length; i++)
		{
			if (playerStreams[i] == null)
			{
				Log($"Player id: {players[i].id} ({players[i].id.Length}), found {id} ({id.Length}) | {players[i].id == id}");
				if (players[i].id == id)
				{
					playersConnected++;
					foundPlayer = true;
					playerStreams[i] = stream;
					stream.WriteByte((byte)i);
				}
			}
		}
		if (!foundPlayer)
		{
			Log("Found no player", severity: LogSeverity.Error);
			//FIXME: Be more nice, see above
			stream.Close();
		}
	}

	private bool HandleGameLogic()
	{
		while (!state.HasFlag(GameConstants.State.InitGained))
		{
			switch (state)
			{
				case GameConstants.State.UNINITIALIZED:
					{
						foreach (Player player in players)
						{
							if(!Program.config.duel_config!.noshuffle)
							{
								player.deck.Shuffle();
							}
							player.Draw(GameConstants.START_HAND_SIZE);
							player.momentum = GameConstants.START_MOMENTUM;
							player.life = GameConstants.START_LIFE;
							player.progress = 0;
						}
						SendFieldUpdates();
						// Mulligan
						for (int i = 0; i < players.Length; i++)
						{
							if (AskYesNo(player: i, question: "Mulligan?"))
							{
								Card[] cards = SelectCardsCustom(i, "Select cards to mulligan", players[i].hand.GetAll(), (x) => true);
								foreach (Card card in cards)
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
						foreach (Player player in players)
						{
							player.Draw(1);
						}
						initPlayer = turnPlayer;
						state = GameConstants.State.MainInitGained;
					}
					break;
				case GameConstants.State.MainInitGained:
					break;
				default:
					throw new NotImplementedException(state.ToString());
			}
			SendFieldUpdates();
		}
		return false;
	}

	private bool HandlePlayerActions()
	{
		for (int i = 0; i < players.Length; i++)
		{
			if (playerStreams[i].DataAvailable)
			{
				List<byte> bytes = ReceiveRawPacket(playerStreams[i])!;
				if (bytes.Count == 0)
				{
					Log("Request was empty, ignoring it", severity: LogSeverity.Warning);
				}
				else
				{
					byte type = bytes[0];
					bytes.RemoveAt(0);
					string packet = Encoding.UTF8.GetString(bytes.ToArray());
					if (HandlePacket(type, packet, i))
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
		if (typeByte >= (byte)NetworkingConstants.PacketType.PACKET_COUNT)
		{
			throw new Exception($"ERROR: Unknown packet type encountered: ({typeByte})");
		}
		NetworkingConstants.PacketType type = (NetworkingConstants.PacketType)typeByte;

		List<byte> payload = new List<byte>();
		switch (type)
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
					if (!GetCardActions(player, request.uid, request.location).Contains(request.desc))
					{
						Log("Tried to use an option that is not present for that card");
					}
					else
					{
						TakeAction(player, request.uid, request.location, request.desc!);
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
		switch (location)
		{
			case GameConstants.Location.Hand:
				{
					Card card = players[player].hand.Get(uid);
					if (option == "Cast")
					{
						Cast(player, card);
					}
					else
					{
						throw new NotImplementedException($"Scripted action {option}");
					}
				}
				break;
			default:
				throw new NotImplementedException($"TakeAction at {location}");
		}
	}

	private string[] GetCardActions(int player, int uid, GameConstants.Location location)
	{
		List<string> options = new List<string>();
		switch (location)
		{
			case GameConstants.Location.Hand:
				Card card = players[player].hand.Get(uid);
				if (card.Cost <= players[player].momentum)
				{
					options.Add("Cast");
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
	private void SendFieldUpdates(GameConstants.Location mask = GameConstants.Location.ALL)
	{
		for (int i = 0; i < players.Length; i++)
		{
			SendFieldUpdate(i, mask);
		}
	}
	public Card[] SelectCardsCustom(int player, string description, Card[] cards, Func<Card[], bool> isValidSelection)
	{
		SendPacketToPlayer(new DuelPackets.CustomSelectCardsRequest
		{
			cards = Card.ToStruct(cards.ToList()),
			desc = description,
			initialState = isValidSelection(new Card[0])
		}, player);

		List<byte> payload = new List<byte>();
		do
		{
			DuelPackets.CustomSelectCardsIntermediateRequest request;
			payload = ReceiveRawPacket(playerStreams[player])!;
			if (payload[0] == (byte)NetworkingConstants.PacketType.DuelCustomSelectCardsResponse)
			{
				break;
			}
			request = DeserializePayload<DuelPackets.CustomSelectCardsIntermediateRequest>(payload);
			SendPacketToPlayer(new DuelPackets.CustomSelectCardsIntermediateResponse
			{
				isValid = isValidSelection(Array.ConvertAll(request.uids, (x => cards.First(y => y.uid == x))))
			}, player);
		} while (true);

		DuelPackets.CustomSelectCardsResponse response = DeserializePayload<DuelPackets.CustomSelectCardsResponse>(payload);
		Card[] ret = cards.Where(x => response.uids.Contains(x.uid)).ToArray();
		if (!isValidSelection(ret))
		{
			throw new Exception("Player somethow selected invalid cards");
		}
		return ret;
	}

	private void SendFieldUpdate(int player, GameConstants.Location mask)
	{
		// TODO: actually handle mask if this is too slow
		DuelPackets.FieldUpdateRequest request = new DuelPackets.FieldUpdateRequest()
		{
			turn = turn + 1,
			hasInitiative = state != GameConstants.State.UNINITIALIZED && initPlayer == player,
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
			},
		};
		SendPacketToPlayer<DuelPackets.FieldUpdateRequest>(request, player);
	}

	private void Cast(int player, Card card)
	{
		switch (card.CardType)
		{
			case GameConstants.CardType.Creature:
				{
					SendPacketToPlayer<DuelPackets.SelectZoneRequest>(new DuelPackets.SelectZoneRequest
					{
						options = players[player].field.GetPlacementOptions(),
					}, player);
					int zone = ReceivePacketFromPlayer<DuelPackets.SelectZoneResponse>(player).zone;
					players[player].CastCreature(card, zone);
				}
				break;
			default:
				throw new NotImplementedException($"Casting {card.CardType} cards");
		}
		SendFieldUpdates();
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