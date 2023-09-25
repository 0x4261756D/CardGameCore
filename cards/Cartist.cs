// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Cartist : Creature
{
	public Cartist() : base(
		Name: "Cartist",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Cast}: Draw 1.",
		OriginalPower: 2,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: () => Draw(player: Controller, amount: 1)), referrer: this);
	}
}
