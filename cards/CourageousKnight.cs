// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CourageousKnight : Creature
{
	public CourageousKnight() : base(
		Name: "Courageous Knight",
		CardClass: PlayerClass.All,
		OriginalCost: 3,
		Text: "{A creature is cast}: Gain +1/+0.",
		OriginalPower: 3,
		OriginalLife: 1
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}