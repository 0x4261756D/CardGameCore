// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class Firecracker : Creature
{
	public Firecracker() : base(
		Name: "Firecracker",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 2,
		Text: "{Cast}: Cast \"Ignite\".",
		OriginalPower: 2,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
	}

	public void CastEffect()
	{
		Cast(Controller, new Ignite() { BaseController = this.Controller, Controller = this.Controller });
	}

}
