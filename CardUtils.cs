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

	public delegate bool IsValid(Card card);
	public static bool ContainsValid(Card[] cards, IsValid isValid)
	{
		return cards.Any(x => isValid(x));
	}
	public static Card[] FilterValid(Card[] cards, IsValid isValid)
	{
		return cards.Where(x => isValid(x)).ToArray();
	}
}