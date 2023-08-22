// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Masterpiece : Spell
{
	public Masterpiece() : base(
		Name: "Masterpiece",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 6,
		Text: "{Cast}: Target a creature in your Grave, create any number X/X token copies of it (min. 1), where X is the sum of the target's Power and Life."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Creature target = (Creature)SelectSingleCard(Controller, FilterValid(cards: GetGrave(Controller), isValid: (card) => card.CardType == CardType.Creature), "Select masterpiece");
		while(HasEmpty(GetField(Controller)) && AskYesNo(player: Controller, question: "Create another?"))
		{
			Creature token = CreateTokenCopy(player: Controller, target);
			CreatureChangeLife(target: token, amount: target.Power, source: this);
			CreatureChangePower(target: token, amount: target.Life, source: this);
			MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
		}
	}

	private bool CastCondition()
	{
		return ContainsValid(cards: GetGrave(Controller), isValid: (card) => card.CardType == CardType.Creature) && HasEmpty(GetField((Controller)));
	}
}
