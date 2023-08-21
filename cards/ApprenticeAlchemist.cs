// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

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
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothWholeFields());
	}

	private void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetForBoth(GetFieldUsed), description: "Select target to \"accidentally\" destroy");
		int controller = target.Controller;
		Destroy(target);
		CreateTokenOnField(player: controller, power: target.BaseLife, life: target.BasePower, name: "Accident", source: this);
	}
}
