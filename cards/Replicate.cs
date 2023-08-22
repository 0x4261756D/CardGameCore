// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Replicate : Spell
{
	public Replicate() : base(
		Name: "Replicate",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Create token copy of your target creature."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, GetFieldUsed(player: Controller), description: "Select card to replicate");
		CreateTokenCopyOnField(player: Controller, card: target, source: this);
	}

	private bool CastCondition()
	{
		return HasEmpty(GetField(Controller)) && HasUsed(GetField(Controller));
	}
}
