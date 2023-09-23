using CardGameUtils;

namespace CardGameCore;

class Grave
{
	private List<Card> cards = new();
	public int Size
	{
		get => cards.Count;
	}

	public Grave()
	{

	}

	internal void Add(Card card)
	{
		if(card.CardType == GameConstants.CardType.Creature && ((Creature)card).Keywords.ContainsKey(Keyword.Token))
		{
			return;
		}
		card.Location = GameConstants.Location.Grave;
		card.ResetToBaseState();
		cards.Add(card);
	}

	internal Card[] GetAll()
	{
		return cards.ToArray();
	}

	internal void Remove(Card card)
	{
		cards.Remove(card);
		card.Location &= ~GameConstants.Location.Grave;
	}
}
