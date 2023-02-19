using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using CardGameUtils;
using static CardGameUtils.Functions;

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
				Log($"Player {Program.config.duel_config.players[i].name} has no ability");
				return;
			}
			Card ability = CreateBasicCard(Type.GetType(Program.config.duel_config.players[i].decklist[1].Substring(1))!, i);
			if (!Program.config.duel_config.players[i].decklist[2].StartsWith("|"))
			{
				Log($"Player {Program.config.duel_config.players[i].name} has no quest");
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

	private bool HandleGameLogic()
	{
		throw new NotImplementedException();
	}

	private bool HandlePlayerActions()
	{
		throw new NotImplementedException();
	}
}