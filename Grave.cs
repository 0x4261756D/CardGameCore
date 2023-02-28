using CardGameUtils;

namespace CardGameCore;

class Grave
{
	private List<Card> cards = new List<Card>();
	public int Size
	{
		get => cards.Count;
	}
	
	public Grave()
	{

	}

	internal void Add(Card card)
	{
		card.Location = GameConstants.Location.Grave;
		cards.Add(card);
	}
}