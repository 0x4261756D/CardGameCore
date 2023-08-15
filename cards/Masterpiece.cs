// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Masterpiece : Spell
{
	public Masterpiece() : base(
		Name: "Masterpiece",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 5,
		Text: "{Cast}: Target a creature in your Grave, create any number X/X token copies of it (min. 1), where X is the sum of the target's Power and Life."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Card target = SelectSingleCard(Controller, GetGrave(Controller), "Select masterpiece");
		while(HasEmpty(GetField(Controller)) && AskYesNo(player: Controller, question: "Create another?"))
		{
			Card token = CreateTokenCopy(player: Controller, target);
			CreatureChangeLife(target: token, amount: target.Power, source: this);
			CreatureChangePower(target: token, amount: target.Life, source: this);
			MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token);
		}
	}

	private bool CastCondition()
	{
		return GetGrave(Controller).Length > 0 && HasEmpty(GetField((Controller)));
	}
}