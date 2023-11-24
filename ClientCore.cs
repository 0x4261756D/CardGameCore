using System.IO.Pipes;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CardGameUtils;
using CardGameUtils.Structs;
using static CardGameUtils.Functions;
using static CardGameUtils.Structs.NetworkingStructs;

namespace CardGameCore;

partial class ClientCore : Core
{
	readonly static List<CardStruct> cards = [];
	readonly static List<DeckPackets.Deck> decks = [];
	public ClientCore()
	{
		if(Program.config.deck_config == null)
		{
			Log("Deck config was null when creating a client core", LogSeverity.Error);
			return;
		}
		if(!Directory.Exists(Program.config.deck_config.deck_location))
		{
			Log($"Deck folder not found, creating it at {Program.config.deck_config.deck_location}", LogSeverity.Warning);
			Directory.CreateDirectory(Program.config.deck_config.deck_location);
		}
		string[] deckfiles = Directory.GetFiles(Program.config.deck_config.deck_location);
		foreach(Type card in Assembly.GetExecutingAssembly().GetTypes().Where(Program.IsCardSubclass))
		{
			Card c = (Card)Activator.CreateInstance(card)!;
			cards.Add(c.ToStruct(client: true));
		}

		if(Program.config.deck_config.should_fetch_additional_cards)
		{
			TryFetchAdditionalCards();
		}

		foreach(string deckfile in deckfiles)
		{
			List<string> decklist = File.ReadAllLines(deckfile).ToList();
			if(decklist.Count == 0)
			{
				continue;
			}
			DeckPackets.Deck deck = new()
			{
				player_class = Enum.Parse<GameConstants.PlayerClass>(decklist[0]),
				name = Path.GetFileNameWithoutExtension(deckfile)
			};
			decklist.RemoveAt(0);
			if(decklist.Count > 0)
			{
				if(decklist[0].StartsWith('#'))
				{
					deck.ability = cards[cards.FindIndex(x => x.name == decklist[0][1..])];
					decklist.RemoveAt(0);
				}
				if(decklist[0].StartsWith('|'))
				{
					deck.quest = cards[cards.FindIndex(x => x.name == decklist[0][1..])];
					decklist.RemoveAt(0);
				}
				deck.cards = DecklistToCards(decklist);
			}
			else
			{
				deck.cards = [];
			}
			decks.Add(deck);
		}
	}

	public override void Init(PipeStream? pipeStream)
	{
		HandleNetworking();
		listener.Stop();
	}

	//TODO: This could be more elegant
	public static CardStruct[] DecklistToCards(List<string> decklist)
	{
		List<CardStruct> c = [];
		foreach(string line in decklist)
		{
			int index = cards.FindIndex(x => x.name == line);
			if(index >= 0)
			{
				c.Add(cards[index]);
			}
		}
		return [.. c];
	}
	public static void TryFetchAdditionalCards()
	{
		try
		{
			using TcpClient client = new(Program.config.deck_config!.additional_cards_url.address, Program.config.deck_config.additional_cards_url.port);
			using NetworkStream stream = client.GetStream();
			stream.Write(GeneratePayload(new ServerPackets.AdditionalCardsRequest()));

			byte[]? response = TryReceivePacket<ServerPackets.AdditionalCardsResponse>(stream, 1000);
			if(response == null)
			{
				return;
			}
			ServerPackets.AdditionalCardsResponse data = DeserializeJson<ServerPackets.AdditionalCardsResponse>(response);
			if(data.time < Program.versionTime)
			{
				Log($"Did not apply additional cards as they were older (client: {Program.versionTime}, server: {data.time})");
				return;
			}
			foreach(CardStruct card in data.cards)
			{
				cards.Remove(card);
				cards.Add(card);
			}
		}
		catch(Exception e)
		{
			Log($"Could not fetch additional cards {e.Message}", severity: LogSeverity.Warning);
		}
	}
	public override void HandleNetworking()
	{
		listener.Start();
		while(true)
		{
			Log("Waiting for a connection");
			using TcpClient client = listener.AcceptTcpClient();
			using NetworkStream stream = client.GetStream();
			(byte type, byte[]? bytes) = ReceiveRawPacket(stream);
			Log("Received a request");
			if(bytes == null || bytes.Length == 0)
			{
				Log("The request was empty, ignoring it", severity: LogSeverity.Warning);
			}
			else
			{
				if(HandlePacket(type, bytes, stream))
				{
					Log("Received a package that says the server should close");
					break;
				}
				Log("Sent a response");
			}
		}
		listener.Stop();
	}

	public bool HandlePacket(byte typeByte, byte[] bytes, NetworkStream stream)
	{
		// THIS MIGHT CHANGE AS SENDING RAW JSON MIGHT BE TOO EXPENSIVE/SLOW
		// possible improvements: Huffman or Burrows-Wheeler+RLE
		if(typeByte >= (byte)NetworkingConstants.PacketType.PACKET_COUNT)
		{
			throw new Exception($"ERROR: Unknown packet type encountered: ({typeByte})");
		}
		NetworkingConstants.PacketType type = (NetworkingConstants.PacketType)typeByte;
		byte[] payload;
		switch(type)
		{
			case NetworkingConstants.PacketType.DeckNamesRequest:
			{
				DeckPackets.NamesRequest request = DeserializeJson<DeckPackets.NamesRequest>(bytes);
				payload = GeneratePayload(new DeckPackets.NamesResponse
				{
					names = [.. decks.ConvertAll(x => x.name)]
				});
			}
			break;
			case NetworkingConstants.PacketType.DeckListRequest:
			{
				DeckPackets.ListRequest request = DeserializeJson<DeckPackets.ListRequest>(bytes);
				payload = GeneratePayload(new DeckPackets.ListResponse
				{
					deck = FindDeckByName(request.name!),
				});
			}
			break;
			case NetworkingConstants.PacketType.DeckSearchRequest:
			{
				DeckPackets.SearchRequest request = DeserializeJson<DeckPackets.SearchRequest>(bytes);
				payload = GeneratePayload(new DeckPackets.SearchResponse
				{
					cards = FilterCards(cards, request.filter!, request.playerClass, request.includeGenericCards)
				});
			}
			break;
			case NetworkingConstants.PacketType.DeckListUpdateRequest:
			{
				DeckPackets.Deck deck = DeserializeJson<DeckPackets.ListUpdateRequest>(bytes).deck;
				deck.name = DeckNameRegex().Replace(deck.name, "");
				int index = decks.FindIndex(x => x.name == deck.name);
				if(deck.cards != null)
				{
					if(index == -1)
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
					if(index != -1)
					{
						decks.RemoveAt(index);
						File.Delete(Path.Combine(Program.config.deck_config!.deck_location, deck.name + ".dek"));
					}
				}
				payload = GeneratePayload(new DeckPackets.ListUpdateResponse { should_update = index == -1 });
			}
			break;
			default:
				throw new Exception($"ERROR: Unable to process this packet: ({type}) | {Encoding.UTF8.GetString(bytes)}");
		}
		stream.Write(payload);
		return false;
	}

	private void SaveDeck(DeckPackets.Deck deck)
	{
		string? deckString = deck.ToString();
		if(deckString == null) return;
		File.WriteAllText(Path.Combine(Program.config.deck_config!.deck_location, deck.name + ".dek"), deckString);
	}

	private CardStruct[] FilterCards(List<CardStruct> cards, string filter, GameConstants.PlayerClass playerClass, bool includeGenericCards)
	{
		return cards.Where(x =>
			(playerClass == GameConstants.PlayerClass.All || (includeGenericCards && x.card_class == GameConstants.PlayerClass.All) || x.card_class == playerClass)
			&& x.ToString().Contains(filter, StringComparison.CurrentCultureIgnoreCase)).ToArray();
	}

	private DeckPackets.Deck FindDeckByName(string name)
	{
		name = DeckNameRegex().Replace(name, "");
		return decks[decks.FindIndex(x => x.name == name)];
	}

	[GeneratedRegex(@"[\./\\]")]
	private static partial Regex DeckNameRegex();
}
