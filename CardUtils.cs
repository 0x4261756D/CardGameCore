using System;

namespace CardGameCore;
public class CardUtils
{
	public static bool HasEmpty(Card?[] cards)
	{
		return Array.Exists(cards, card => card == null);
	}

	public static bool HasUsed(Card?[] cards)
	{
		return Array.Exists(cards, card => card != null);
	}

	public static Card[] GetForBoth(GetCardsInLocationDelegate accessor)
	{
		return [.. accessor(0), .. accessor(1)];
	}
	public static Creature?[] GetBothFieldsWhole()
	{
		return [.. Card.GetField(0), .. Card.GetField(1)];
	}

	public delegate bool IsValid<T>(T card);
	public static bool ContainsValid<T>(T[] cards, IsValid<T> isValid) where T : Card
	{
		return Array.Exists(cards, card => isValid(card));
	}
	public static T[] FilterValid<T>(T[] cards, IsValid<T> isValid) where T : Card
	{
		return Array.FindAll(cards, card => isValid(card));
	}
	public static Creature[] GetBothFieldsUsed()
	{
		return [.. Card.GetFieldUsed(0), .. Card.GetFieldUsed(1)];
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
