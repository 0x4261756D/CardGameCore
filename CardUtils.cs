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

	public static Card[] GetForBoth(GetCardsInLocationDelegate accessor)
	{
		return accessor(0).Concat(accessor(1)).ToArray();
	}
	public static Card?[] GetBothFieldsWhole()
	{
		return Card.GetField(0).Concat(Card.GetField(1)).ToArray();
	}

	public delegate bool IsValid<T>(T card);
	public static bool ContainsValid<T>(T[] cards, IsValid<T> isValid) where T : Card
	{
		return cards.Any(x => isValid(x));
	}
	public static T[] FilterValid<T>(T[] cards, IsValid<T> isValid) where T : Card
	{
		return cards.Where(x => isValid(x)).ToArray();
	}
	public static Creature[] GetBothFieldsUsed()
	{
		return Card.GetFieldUsed(0).Concat(Card.GetFieldUsed(1)).ToArray();
	}

	public static void ChangeLifeOfAnyTarget(int player, int amount, Card source, string description = "Change life of")
	{
		Creature[] fields = GetBothFieldsUsed();
		if(fields.Length > 0 && Card.AskYesNo(player: player, question: description + " a creature?"))
		{
			Creature target = SelectSingleCard(player: player, cards: fields, description: "Select target to " + description.ToLower());
			Card.CreatureChangeLife(target, amount, source);
		}
		else
		{
			Card.PlayerChangeLife(player: Card.AskYesNo(player: player, question: description + " the opponent?") ? 1 - player : player, amount: amount, source: source);
		}
	}

	public static T SelectSingleCard<T>(int player, T[] cards, string description) where T : Card
	{
		return (T)Card.SelectCards(player, cards, 1, description)[0];
	}
}
