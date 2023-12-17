// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class ApprenticeAlchemist : Creature
{
	public ApprenticeAlchemist() : base(
		Name: "Apprentice Alchemist",
		CardClass: PlayerClass.All,
		OriginalCost: 4,
		Text: "{Cast}: Destroy target creature, its controller creates an Accident token with the target's power and life",
		OriginalPower: 3,
		OriginalLife: 6
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothFieldsWhole());
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Select target to \"accidentally\" destroy");
		int controller = target.Controller;
		Destroy(target);
		CreateTokenOnField(player: controller, power: target.BaseLife, life: target.BasePower, name: "Accident", source: this);
	}
}
