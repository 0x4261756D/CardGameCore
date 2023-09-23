using System.Text.Json;
using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Field
{
	private Creature?[] cards = new Creature?[GameConstants.FIELD_SIZE];
	public Field()
	{

	}

	internal CardStruct?[] ToStruct()
	{
		return cards.ToList().ConvertAll(x => x?.ToStruct()).ToArray();
	}

	internal Creature?[] GetAll()
	{
		return cards;
	}
	internal Creature[] GetUsed()
	{
		return cards.Where(x => x != null).ToArray()!;
	}

	internal void Add(Creature card, int zone)
	{
		if(cards[zone] != null)
		{
			throw new Exception($"Tried to move {card} to zone {zone} occupied by {cards[zone]}");
		}
		card.Location = GameConstants.Location.Field;
		card.Position = zone;
		cards[zone] = card;
	}

	internal bool[] GetMovementOptions(int position, int momentum)
	{
		bool[] ret = new bool[GameConstants.FIELD_SIZE];
		Creature? card = cards[position];
		if(card == null)
		{
			Functions.Log($"Called GetMovementOptions for an empty position", severity: Functions.LogSeverity.Warning);
			return ret;
		}
		int movementCost = card.CalculateMovementCost();
		for(int i = Math.Max(0, position - (momentum / movementCost)); i < position; i++)
		{
			if(cards[i] == null)
			{
				ret[i] = true;
			}
		}
		for(int i = Math.Min(GameConstants.FIELD_SIZE - 1, position + 1);
			i <= Math.Min(GameConstants.FIELD_SIZE - 1, position + (momentum / movementCost)); i++)
		{
			if(cards[i] == null)
			{
				ret[i] = true;
			}
		}
		return ret;
	}
	internal bool[] GetPlacementOptions()
	{
		return cards.ToList().ConvertAll(x => x == null).ToArray();
	}

	internal void ClearCardModifications()
	{
		foreach(Creature? card in cards)
		{
			card?.ResetToBaseState();
		}
	}

	internal bool HasEmpty()
	{
		return cards.Any(x => x == null);
	}

	internal Creature GetByUID(int uid)
	{
		foreach(Creature? card in cards)
		{
			if(card?.uid == uid)
			{
				return card;
			}
		}
		throw new Exception($"Could not find card with UID {uid} on the field");
	}
	internal Creature? GetByPosition(int position)
	{
		if(position < 0 || position >= cards.Length)
		{
			throw new Exception($"Field position oob {position}");
		}
		return cards[position];
	}

	internal bool CanMove(int position, int momentum)
	{
		Creature? card = cards[position];
		if(card == null)
		{
			Functions.Log($"Called CanMove for an empty position", severity: Functions.LogSeverity.Warning);
			return false;
		}
		if(card.Keywords.ContainsKey(Keyword.Immovable))
		{
			return false;
		}
		int movementCost = 1 + card.Keywords.GetValueOrDefault(Keyword.Colossal, 0);
		for(int i = Math.Max(0, position - (momentum / movementCost)); i < position; i++)
		{
			if(cards[i] == null)
			{
				return true;
			}
		}
		for(int i = Math.Min(GameConstants.FIELD_SIZE - 1, position + 1);
			i <= Math.Min(GameConstants.FIELD_SIZE - 1, position + (momentum / movementCost)); i++)
		{
			if(cards[i] == null)
			{
				return true;
			}
		}
		return false;
	}

	internal void Move(int position, int zone)
	{
		Creature? card = cards[position];
		if(card == null)
		{
			Functions.Log($"Called Move for an empty position", severity: Functions.LogSeverity.Warning);
			return;
		}
		if(cards[zone] != null)
		{
			Functions.Log($"Called Move to move to an occupied position", severity: Functions.LogSeverity.Error);
			return;
		}
		cards[position] = null;
		cards[zone] = card;
		card.Position = zone;
	}
	internal void Remove(Creature card)
	{
		for(int i = 0; i < GameConstants.FIELD_SIZE; i++)
		{
			if(cards[i] != null && cards[i] == card)
			{
				cards[i] = null;
				card.Location &= ~GameConstants.Location.Field;
				return;
			}
		}
		if(Program.replay != null)
		{
			string replayPath = Path.Combine(Program.baseDir, "replays");
			Directory.CreateDirectory(replayPath);
			string filePath = Path.Combine(replayPath, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_Field_Remove_Bug_{Program.config.duel_config?.players[0].name}_vs_{Program.config.duel_config?.players[1].name}.replay");
			File.WriteAllText(filePath, JsonSerializer.Serialize(Program.replay, NetworkingConstants.jsonIncludeOption));
			Functions.Log("Wrote replay to " + filePath);
		}

		throw new Exception($"Could not remove {card} ({card.uid}) from the field because it was not present (location: {card.Location}, position: {card.Position}, controller. {card.Controller}/{card.BaseController})");
	}
}
