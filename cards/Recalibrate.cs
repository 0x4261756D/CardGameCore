// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Recalibrate : Spell
{
	public Recalibrate() : base(
		Name: "Recalibrate",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Take control of target creature. It gains [Brittle]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, GetFieldUsed(player: 1 - Controller), description: "Select card to recalibrate");
		target.RegisterKeyword(Keyword.Brittle);
		MoveToField(choosingPlayer: Controller, targetPlayer: Controller, card: target, source: this);
	}

	private bool CastCondition()
	{
		return HasEmpty(GetField(Controller)) && HasUsed(GetField(1 - Controller));
	}
}
