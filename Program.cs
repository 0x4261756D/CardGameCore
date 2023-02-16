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
	public static void Main(string[] args)
	{
		string? configPath = null;
		for (int i = 0; i < args.Length; i++)
		{
			string[] parts = args[i].Split('=');
			if (parts.Length == 2)
			{
				string path = Path.Combine(baseDir, parts[1]);
				switch (parts[0])
				{
					case "--config":
						if (File.Exists(path))
						{
							configPath = path;
						}
						else
						{
							Log($"No config file found at {path}.", severity: LogSeverity.Error);
							return;
						}
						break;
					case "--additional_cards_path":
						//GenerateAdditionalCards(path);
						Log("Done generating new additional cards");
						break;
				}
			}
		}
		if (configPath == null)
		{
			Log("Please supply a config location with '--config=<path/to/config.json>'");
			return;
		}
		if (!File.Exists(configPath))
		{
			Log($"Missing a config at {configPath}.", severity: LogSeverity.Error);
			return;
		}
		PlatformCoreConfig? platformConfig = JsonSerializer.Deserialize<PlatformCoreConfig>(File.ReadAllText(configPath), NetworkingConstants.jsonIncludeOption);
		if (platformConfig == null)
		{
			Log("Could not parse a platform config", LogSeverity.Error);
			return;
		}
		if (Environment.OSVersion.Platform == PlatformID.Unix)
		{
			config = platformConfig.linux!;
		}
		else
		{
			config = platformConfig.windows!;
		}
		bool modeSet = false;
		foreach (string s in args)
		{
			if (s.StartsWith("--"))
			{
				string arg = s.Substring(2).Split('=')[0];
				string parameter = s.Substring(arg.Length + 3);
				switch (arg)
				{
					case "mode":
						if (parameter == "duel")
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
						if (!players.StartsWith("µ") || !players.EndsWith("µ"))
						{
							Log($"Your players string is in a wrong format: {players}", severity: LogSeverity.Error);
							return;
						}
						string[] playerData = players.Split('µ');
						if (playerData.Length != 8)
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
		if (config.mode == CoreConfig.CoreMode.Client)
		{
			new ClientCore();
		}
		else if(config.mode == CoreConfig.CoreMode.Duel)
		{
			new DuelCore();
		}
		Log("EXITING");
	}
	public static readonly Func<Type, bool> IsCardSubclass = (x) =>
	{
		return x.BaseType == typeof(Spell) || x.BaseType == typeof(Creature) || x.BaseType == typeof(Quest);
	};
}