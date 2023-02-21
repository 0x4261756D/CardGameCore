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
}