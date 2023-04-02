// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class BrilliantMedic : Creature
{
	public BrilliantMedic() : base(
		Name: "Brilliant Medic",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Cast}: Heal target any target by 4.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: HealEffect, condition: () => true), referrer: this);
	}

	public void HealEffect()
	{
		ChangeLifeOfAnyTarget(player: Controller, amount: 4, description: "Heal");
	}
}