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
}