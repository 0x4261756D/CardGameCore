using System;
using System.Collections.Generic;
using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Field
{
	private readonly Creature?[] cards = new Creature?[GameConstants.FIELD_SIZE];
	public Field()
	{

	}

	internal CardStruct?[] ToStruct()
	{
		return Array.ConvertAll(cards, card => card?.ToStruct());
	}

	internal Creature?[] GetAll()
	{
		return cards;
	}
	internal Creature[] GetUsed()
	{
		return Array.FindAll(cards, card => card != null)!;
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
		return Array.ConvertAll(cards, x => x == null);
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
		return Array.Exists(cards, card => card == null);
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

		throw new Exception($"Could not remove {card} ({card.uid}) from the field because it was not present (location: {card.Location}, position: {card.Position}, controller. {card.Controller}/{card.BaseController})");
	}
}
