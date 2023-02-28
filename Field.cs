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

	internal bool[] GetPlacementOptions()
	{
		return cards.ToList().ConvertAll(x => x == null).ToArray();
	}

	internal void ClearCardModifications()
	{
		foreach (Card? card in cards)
		{
			card?.ClearModifications();
		}
	}

}