namespace CardGameCore;
using static CardGameUtils.Functions;
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
	public static Card?[] GetBothWholeFields()
	{
		return Card.GetField(0).Concat(Card.GetField(1)).ToArray();
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

	public static void ChangeLifeOfAnyTarget(int player, int amount, Card source, string description = "Change life of")
	{
		Card[] fields = GetForBoth(Card.GetFieldUsed);
		if(fields.Length > 0 && Card.AskYesNo(player: player, question: description + " a creature?"))
		{
			Card target = SelectSingleCard(player: player, cards: fields, description: "Select target to " + description.ToLower());
			Card.RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: (_) => target.Life += amount, referrer: target));
		}
		else
		{
			Card.PlayerChangeLife(player: Card.AskYesNo(player: player, question: description + " the opponent?") ? 1 - player : player, amount: amount, source: source);
		}
	}

	public static Card SelectSingleCard(int player, Card[] cards, string description)
	{
		return Card.SelectCards(player, cards, 1, description)[0];
	}
}
