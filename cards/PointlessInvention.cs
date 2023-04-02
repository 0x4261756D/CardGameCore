// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class PointlessInvention : Creature
{
	public PointlessInvention() : base(
		Name: "Pointless Invention",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "This creature only takes damage in increments of 1.",
		OriginalPower: 5,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		this.damageCap = 1;
	}

}