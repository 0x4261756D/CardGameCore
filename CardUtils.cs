namespace CardGameCore;

public class CardUtils
{
	public static bool HasEmpty(Card?[] cards)
	{
		return cards.Any(x => x == null);
	}

	public static bool HasUsed(Card?[] cards)
	{
		return cards.Any(x => x != null);
	}
}