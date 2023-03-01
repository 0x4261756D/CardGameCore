using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Hand
{
	private List<Card> cards = new List<Card>();
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
		return cards.ConvertAll(x => x.ToStruct()).ToArray();
	}

	internal Card[] GetAll()
	{
		return cards.ToArray();
	}

	internal void Remove(Card c)
	{
		c.Location = GameConstants.Location.UNKNOWN;
		cards.Remove(c);
	}

	internal CardStruct[] ToHiddenStruct()
	{
		return cards.ConvertAll(x => new CardStruct()).ToArray();
	}

	internal Card GetByUID(int uid)
	{
		return cards[cards.FindIndex(x => x.uid == uid)];
	}

	internal void ClearCardModifications()
	{
		foreach(Card card in cards)
		{
			card.ClearModifications();
		}
	}
}