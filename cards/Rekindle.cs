// scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Rekindle : Spell
{
	public Rekindle() : base(
		Name: "Rekindle",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Draw X, where X is the amount of Damage \"Ignite\" would deal.\n{Revelation}: Draw 1."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(() => Draw(player: Controller, amount: 1)), referrer: this);
	}

	public void CastEffect()
	{
		Draw(player: Controller, amount: GetIgniteDamage(Controller));
	}

}
