// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

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
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}
	private void CreateMasterpiece(Creature target)
	{
		Creature token = CreateTokenCopy(player: Controller, target);
		CreatureChangeLife(target: token, amount: target.Power, source: this);
		CreatureChangePower(target: token, amount: target.Life, source: this);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
	}
	private void CastEffect()
	{
		Creature target = (Creature)SelectSingleCard(Controller, FilterValid(cards: GetGrave(Controller), isValid: (card) => card.CardType == CardType.Creature), "Select masterpiece");
		if(HasEmpty(GetField(Controller)) && AskYesNo(player: Controller, question: "Create a masterpiece?"))
		{
			CreateMasterpiece(target);
		}
		while(HasEmpty(GetField(Controller)) && AskYesNo(player: Controller, question: "Create another?"))
		{
			CreateMasterpiece(target);
		}
	}

	private bool CastCondition()
	{
		return ContainsValid(cards: GetGrave(Controller), isValid: (card) => card.CardType == CardType.Creature) && HasEmpty(GetField(Controller));
	}
}
