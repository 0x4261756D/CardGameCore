using CardGameUtils;

namespace CardGameCore;

class Deck
{
	private List<Card> cards = new List<Card>();
	public Deck()
	{

	}

	public int Size
	{
		get => cards.Count;
	}

	internal void Add(Card c)
	{
		c.Location = GameConstants.Location.Deck;
		cards.Add(c);
	}

	internal Card Pop()
	{
		Card ret = cards[0];
		cards.RemoveAt(0);
		ret.Location = GameConstants.Location.UNKNOWN;
		return ret;
	}

	internal void Shuffle()
	{
		for (int i = cards.Count - 1; i >= 0; i--)
		{
			int k = DuelCore.rnd.Next(i);
			Card tmp = cards[i];
			cards[i] = cards[k];
			cards[k] = tmp;
		}
	}
}