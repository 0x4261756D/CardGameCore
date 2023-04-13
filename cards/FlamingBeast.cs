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
		RegisterAttackTrigger(trigger: new Trigger(effect: AttackEffect), referrer: this);
	}

	private void AttackEffect()
	{
		Cast(player: Controller, card: new Ignite() { Controller = Controller });
	}
}