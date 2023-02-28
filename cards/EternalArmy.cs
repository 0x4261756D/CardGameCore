// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EternalArmy : Spell
{
	public EternalArmy() : base(
		Name: "Eternal Army",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 6,
		Text: "{Cast}: Create any number of 5/1 Construct tokens with \"{Death}: Create 1 1/1 Construct.\"."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}