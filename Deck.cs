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

	internal Card? Pop()
	{
		if(cards.Count == 0) return null;
		Card ret = cards[0];
		cards.RemoveAt(0);
		ret.Location = GameConstants.Location.UNKNOWN;
		return ret;
	}

	internal Card GetAt(int position)
	{
		return cards[position];
	}

	internal void MoveToBottom(int position)
	{
		Card c = cards[position];
		cards.RemoveAt(position);
		cards.Add(c);
	}

	internal Card[] GetRange(int position, int amount)
	{
		return cards.GetRange(position, amount).ToArray();
	}

	internal void Remove(Card card)
	{
		if(!cards.Remove(card))
		{
			throw new Exception($"Tried to remove nonexistent card {card} from the deck");
		}
		card.Location &= ~GameConstants.Location.Deck;
	}

	internal void Shuffle()
	{
		for(int i = cards.Count - 1; i >= 0; i--)
		{
			int k = DuelCore.rnd.Next(i);
			Card tmp = cards[i];
			cards[i] = cards[k];
			cards[k] = tmp;
		}
	}
}