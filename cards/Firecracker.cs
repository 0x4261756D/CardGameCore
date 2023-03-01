// Scripted by 0x4261756D
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
	// TODO: implement functionality

	public override void Init()
	{
	}

}