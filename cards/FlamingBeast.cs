// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class FlamingBeast : Creature
{
	public FlamingBeast() : base(
		Name: "Flaming Beast",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Attack}: Cast \"Ignite\"",
		OriginalPower: 3,
		OriginalLife: 6
		)
	{ }

	public override void Init()
	{
		RegisterAttackTrigger(trigger: new CreatureTargetingTrigger(effect: AttackEffect, influenceLocation: Location.Field), referrer: this);
	}

	private void AttackEffect(Creature target)
	{
		Cast(player: Controller, card: new Ignite() { BaseController = target.Controller, Controller = target.Controller });
	}
}
