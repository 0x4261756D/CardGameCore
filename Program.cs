using System.Reflection;
using System.Text;
using System.Text.Json;
using CardGameUtils;
using CardGameUtils.Structs;
using static CardGameUtils.Functions;
namespace CardGameCore;

class Program
{
	public static string baseDir = AppDomain.CurrentDomain.BaseDirectory;
	public static CoreConfig config = new CoreConfig(-1, CoreConfig.CoreMode.Client);
	public static Replay? replay;
	public static int seed;
	public static void Main(string[] args)
	{
		seed = new Random().Next();
		string? configPath = null;
		for(int i = 0; i < args.Length; i++)
		{
			string[] parts = args[i].Split('=');
			if(parts.Length == 2)
			{
				string path = Path.Combine(baseDir, parts[1]);
				switch(parts[0])
				{
					case "--config":
						if(File.Exists(path))
						{
							configPath = path;
						}
						else
						{
							Log($"No config file found at {Path.GetFullPath(path)}.", severity: LogSeverity.Error);
							return;
						}
						break;
					case "--additional_cards_path":
						GenerateAdditionalCards(path);
						Log("Done generating new additional cards");
						return;
				}
			}
		}
		if(configPath == null)
		{
			Log("Please supply a config location with '--config=<path/to/config.json>'");
			return;
		}
		if(!File.Exists(configPath))
		{
			Log($"Missing a config at {configPath}.", severity: LogSeverity.Error);
			return;
		}
		PlatformCoreConfig? platformConfig = JsonSerializer.Deserialize<PlatformCoreConfig>(File.ReadAllText(configPath), NetworkingConstants.jsonIncludeOption);
		if(platformConfig == null)
		{
			Log("Could not parse a platform config", LogSeverity.Error);
			return;
		}
		if(Environment.OSVersion.Platform == PlatformID.Unix)
		{
			config = platformConfig.linux!;
		}
		else
		{
			config = platformConfig.windows!;
		}
		bool modeSet = false;
		foreach(string s in args)
		{
			if(s.StartsWith("--"))
			{
				string arg = s.Substring(2).Split('=')[0];
				string parameter = s.Substring(arg.Length + 3);
				switch(arg)
				{
					case "mode":
						if(parameter == "duel")
						{
							config.mode = CoreConfig.CoreMode.Duel;
						}
						else
						{
							config.mode = CoreConfig.CoreMode.Client;
						}
						modeSet = true;
						break;
					case "players":
						string players = Encoding.UTF8.GetString(Convert.FromBase64String(parameter));
						if(!players.StartsWith("µ") || !players.EndsWith("µ"))
						{
							Log($"Your players string is in a wrong format: {players}", severity: LogSeverity.Error);
							return;
						}
						string[] playerData = players.Split('µ');
						if(playerData.Length != 8)
						{
							Log($"Your players string is in a wrong format: {players}", severity: LogSeverity.Error);
							return;
						}
						CoreConfig.PlayerConfig[] playerConfigs = new CoreConfig.PlayerConfig[2];
						playerConfigs[0] = new CoreConfig.PlayerConfig(name: playerData[1], id: playerData[2], decklist: playerData[3].Split(';'));
						playerConfigs[1] = new CoreConfig.PlayerConfig(name: playerData[4], id: playerData[5], decklist: playerData[6].Split(';'));
						if(config.duel_config == null)
						{
							config.duel_config = new CoreConfig.DuelConfig(players: playerConfigs, noshuffle: false);
						}
						else
						{
							config.duel_config.players = playerConfigs;
						}
						break;
					case "noshuffle":
						if(config.duel_config != null)
						{
							config.duel_config.noshuffle = Convert.ToBoolean(parameter);
						}
						break;
					case "port":
						config.port = Convert.ToInt32(parameter);
						break;
					case "config":
						break;
					case "replay":
						Log("Recording replay");
						replay = new Replay(args, seed);
						break;
					default:
						Log("Unknown argument " + s, severity: LogSeverity.Error);
						return;
				}
			}
		}
		if(!modeSet)
		{
			Log("No mode supplied, please do so with --mode={client|duel|test}");
			return;
		}
		Core core;
		if(config.mode == CoreConfig.CoreMode.Client)
		{
			core = new ClientCore();
		}
		else
		{
			core = new DuelCore();
		}
		core.Init();
		Log("EXITING");
		if(replay != null)
		{
			string replayPath = Path.Combine(baseDir, "replays");
			Directory.CreateDirectory(replayPath);
			string filePath = Path.Combine(replayPath, $"{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}_{config.duel_config?.players[0].name}_vs_{config.duel_config?.players[1].name}.replay");
			File.WriteAllText(filePath, JsonSerializer.Serialize<Replay>(replay, NetworkingConstants.jsonIncludeOption));
			Log("Wrote replay to " + filePath);
		}
	}

	public static void GenerateAdditionalCards(string path)
	{
		if(!File.Exists(path) || File.GetLastWriteTime(path) < Directory.GetLastWriteTime(baseDir))
		{
			Log($"Creating a new additional cards file ({File.GetLastWriteTime(path)} is before {Directory.GetLastWriteTime(baseDir)})");
			List<CardStruct> cards = new List<CardStruct>();
			foreach(Type card in Assembly.GetExecutingAssembly().GetTypes().Where(Program.IsCardSubclass))
			{
				Card c = (Card)Activator.CreateInstance(card)!;
				cards.Add(c.ToStruct());
			}
			File.WriteAllText(path, JsonSerializer.Serialize(new NetworkingStructs.ServerPackets.AdditionalCardsResponse
			{
				cards = cards.ToArray()
			}, NetworkingConstants.jsonIncludeOption));
		}
	}

	public static readonly Func<Type, bool> IsCardSubclass = (x) =>
	{
		return x != typeof(Token) && (x.BaseType == typeof(Spell) || x.BaseType == typeof(Creature) || x.BaseType == typeof(Quest));
	};
}