using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Field
{
	private Card?[] cards = new Card?[GameConstants.FIELD_SIZE];
	public Field()
	{

	}

	internal CardStruct?[] ToStruct()
	{
		return cards.ToList().ConvertAll(x => x?.ToStruct()).ToArray();
	}

	internal Card?[] GetAll()
	{
		return cards;
	}

	internal void Add(Card card, int zone)
	{
		Functions.Log($"{zone}, {GameConstants.FIELD_SIZE}");
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
		Card? card = cards[position];
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
		foreach(Card? card in cards)
		{
			card?.ClearModifications();
		}
	}

	internal bool HasEmpty()
	{
		return cards.Any(x => x == null);
	}

	internal Card GetByUID(int uid)
	{
		foreach(Card? card in cards)
		{
			if(card?.uid == uid)
			{
				return card;
			}
		}
		throw new Exception($"Could not find card with UID {uid} on the field");
	}

	internal bool CanMove(int position, int momentum)
	{
		Card? card = cards[position];
		if(card == null)
		{
			Functions.Log($"Called CanMove for an empty position", severity: Functions.LogSeverity.Warning);
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
		Card? card = cards[position];
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
}