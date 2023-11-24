using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Hand
{
	private readonly List<Card> cards = [];
	public Hand()
	{

	}

	public void Add(Card c)
	{
		c.Location = GameConstants.Location.Hand;
		cards.Add(c);
	}

	internal CardStruct[] ToStruct()
	{
		return [.. cards.ConvertAll(x => x.ToStruct())];
	}

	internal Card[] GetAll()
	{
		return [.. cards];
	}

	internal void Remove(Card c)
	{
		// NOTE: This could be the wrong workaround, if something like a card cast from nowhere sticks in hand, look here
		if(c.Location != GameConstants.Location.Hand)
		{
			c.Location &= ~GameConstants.Location.Hand;
		}
		cards.Remove(c);
	}

	internal Card[] GetDiscardable(Card? ignore)
	{
		return [.. cards.Where(card => card.uid != ignore?.uid && card.CanBeDiscarded())];
	}

	internal CardStruct[] ToHiddenStruct()
	{
		return [.. cards.ConvertAll(x => new CardStruct())];
	}

	internal Card GetByUID(int uid)
	{
		return cards[cards.FindIndex(x => x.uid == uid)];
	}

	internal void ClearCardModifications()
	{
		foreach(Card card in cards)
		{
			card.ResetToBaseState();
		}
	}
}