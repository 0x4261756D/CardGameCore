using System.Net.Sockets;
using System.Reflection;
using System.Text;
using CardGameUtils;
using CardGameUtils.Structs;
using static CardGameUtils.Functions;
using static CardGameUtils.Structs.NetworkingStructs;

namespace CardGameCore;

class ClientCore : Core
{
	readonly static List<CardGameUtils.Structs.CardStruct> cards = new List<CardGameUtils.Structs.CardStruct>();
	readonly static List<DeckPackets.Deck> decks = new List<DeckPackets.Deck>();
	public ClientCore()
	{
		if (Program.config.deck_config == null)
		{
			Functions.Log("Deck config was null when creating a client core", Functions.LogSeverity.Error);
			return;
		}
		string[] deckfiles = Directory.GetFiles(Program.config.deck_config.deck_location);
		foreach (Type card in Assembly.GetExecutingAssembly().GetTypes().Where(Program.IsCardSubclass))
		{
			Card c = (Card)Activator.CreateInstance(card)!;
			cards.Add(c.ToStruct());
		}

		if (Program.config.deck_config.should_fetch_additional_cards)
		{
			FetchAdditionalCards();
		}

		foreach (string deckfile in deckfiles)
		{
			List<string> decklist = File.ReadAllLines(deckfile).ToList();
			DeckPackets.Deck deck = new CardGameUtils.Structs.NetworkingStructs.DeckPackets.Deck
			{
				player_class = Enum.Parse<GameConstants.PlayerClass>(decklist[0]),
				name = Path.GetFileNameWithoutExtension(deckfile)
			};
			decklist.RemoveAt(0);
			if(decklist[0].StartsWith("#"))
			{
				deck.ability = cards[cards.FindIndex(x => x.name == decklist[0].Substring(1))];
				decklist.RemoveAt(0);
			}
			if(decklist[0].StartsWith("|"))
			{
				deck.quest = cards[cards.FindIndex(x => x.name == decklist[0].Substring(1))];
				decklist.RemoveAt(0);
			}
			deck.cards = DecklistToCards(decklist);
			decks.Add(deck);
		}
		HandleNetworking();
	}

	//TODO: This could be more elegant
	public static CardGameUtils.Structs.CardStruct[] DecklistToCards(List<string> decklist)
	{
		List<CardGameUtils.Structs.CardStruct> c = new List<CardGameUtils.Structs.CardStruct>();
		foreach (string line in decklist)
		{
			c.Add(cards[cards.FindIndex(x => x.name == line)]);
		}
		return c.ToArray();
	}
	public void FetchAdditionalCards()
	{
		try
		{
			using (TcpClient client = new TcpClient(Program.config.deck_config!.additional_cards_url.address, Program.config.deck_config.additional_cards_url.port))
			{
				using (NetworkStream stream = client.GetStream())
				{
					List<byte> payload = GeneratePayload<ServerPackets.AdditionalCardsRequest>(new ServerPackets.AdditionalCardsRequest());
					stream.Write(payload.ToArray(), 0, payload.Count);
					CardGameUtils.Structs.CardStruct[]? list = ReceivePacket<ServerPackets.AdditionalCardsResponse>(stream, 3000)?.cards;
					if (list == null)
					{
						return;
					}
					foreach (CardGameUtils.Structs.CardStruct card in list)
					{
						cards.Remove(card);
						cards.Add(card);
					}
				}
			}
		}
		catch (Exception e)
		{
			Log($"Could not fetch additional cards {e.Message}", severity: LogSeverity.Warning);
		}
	}
	public override void HandleNetworking()
	{
		listener.Start();
		List<byte> bytes = new List<byte>();
		while (true)
		{
			Log("Waiting for a connection");
			TcpClient client = listener.AcceptTcpClient();
			Log("Connected");
			using (NetworkStream stream = client.GetStream())
			{
				bytes = ReceiveRawPacket(stream)!;
				Log("Received a request");
				if (bytes.Count == 0)
				{
					Log("The request was empty, ignoring it", severity: LogSeverity.Warning);
				}
				else
				{
					if (HandlePacket(bytes, stream))
					{
						Log("Received a package that says the server should close");
						break;
					}
					Log("Sent a response");
				}
				stream.Close();
				client.Close();
			}
		}
		listener.Stop();
	}

	public bool HandlePacket(List<byte> bytes, NetworkStream stream)
	{
		// THIS MIGHT CHANGE AS SENDING RAW JSON MIGHT BE TOO EXPENSIVE/SLOW
		// possible improvements: Huffman or Burrows-Wheeler+RLE
		byte type = bytes[0];
		bytes.RemoveAt(0);
		string packet = Encoding.UTF8.GetString(bytes.ToArray());
		List<byte> payload = new List<byte>();
		if (type >= NetworkingConstants.PACKET_COUNT)
		{
			throw new Exception($"ERROR: Unknown packet type encountered: {NetworkingConstants.PacketTypeToName(type)}({type}) | {packet}");
		}
		else if (type == NetworkingConstants.PACKET_DECK_NAMES_REQUEST)
		{
			DeckPackets.NamesRequest request = DeserializeJson<DeckPackets.NamesRequest>(packet);
			payload = GeneratePayload<DeckPackets.NamesResponse>(new DeckPackets.NamesResponse
			{
				names = decks.ConvertAll(x => x.name).ToArray()
			});
		}
		else if (type == NetworkingConstants.PACKET_DECK_LIST_REQUEST)
		{
			DeckPackets.ListRequest request = DeserializeJson<DeckPackets.ListRequest>(packet);
			payload = GeneratePayload<DeckPackets.ListResponse>(new DeckPackets.ListResponse
			{
				deck = FindDeckByName(request.name!),
			});
		}
		else if (type == NetworkingConstants.PACKET_DECK_SEARCH_REQUEST)
		{
			DeckPackets.SearchRequest request = DeserializeJson<DeckPackets.SearchRequest>(packet);
			payload = GeneratePayload<DeckPackets.SearchResponse>(new DeckPackets.SearchResponse
			{
				cards = FilterCards(cards, request.filter!, request.playerClass).ToArray()
			});
		}
		else if (type == NetworkingConstants.PACKET_DECK_LIST_UPDATE_REQUEST)
		{
			DeckPackets.Deck deck = DeserializeJson<DeckPackets.ListUpdateRequest>(packet).deck;
			int index = decks.FindIndex(x => x.name == deck.name);
			if (deck.cards != null)
			{
				if (index == -1)
				{
					decks.Add(deck);
				}
				else
				{
					decks[index] = deck;
				}
				SaveDeck(deck);
			}
			else
			{
				if (index != -1)
				{
					decks.RemoveAt(index);
					File.Delete(Path.Combine(Program.config.deck_config!.deck_location, deck.name + ".dek"));
				}
			}
			payload = GeneratePayload<DeckPackets.ListUpdateResponse>(new DeckPackets.ListUpdateResponse { should_update = index == -1 });
		}
		else
		{
			throw new Exception($"ERROR: Unable to process this packet: {type} | {packet}");
		}
		stream.Write(payload.ToArray(), 0, payload.Count);
		return false;
	}

	private void SaveDeck(DeckPackets.Deck deck)
	{
		if(deck.name == "") return;
		StringBuilder builder = new StringBuilder();
		builder.Append(deck.player_class);
		if(deck.ability != null)
		{
			builder.Append("\n#");
			builder.Append(deck.ability.name);
		}
		if(deck.quest != null)
		{
			builder.Append("\n|");
			builder.Append(deck.quest.name);
		}
		foreach (var card in deck.cards)
		{
			builder.Append("\n");
			builder.Append(card.name);
		}
		File.WriteAllText(Path.Combine(Program.config.deck_config!.deck_location, deck.name + ".dek"), builder.ToString());
	}

	private List<CardStruct> FilterCards(List<CardStruct> cards, string filter, GameConstants.PlayerClass playerClass)
	{
		return cards.Where(x => 
			(playerClass == GameConstants.PlayerClass.All || x.card_class == GameConstants.PlayerClass.All || x.card_class == playerClass)
			&& x.ToString().ToLower().Contains(filter)).ToList();
	}

	private DeckPackets.Deck FindDeckByName(string name)
	{
		return decks[decks.FindIndex(x => x.name == name)];
	}
}