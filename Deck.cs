namespace CardGameCore;

class Deck
{
	private List<Card> cards = new List<Card>();
	public Deck()
	{

	}

	internal void Add(Card c)
	{
		cards.Add(c);
	}

	internal Card Pop()
	{
		Card ret = cards[0];
		cards.RemoveAt(0);
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