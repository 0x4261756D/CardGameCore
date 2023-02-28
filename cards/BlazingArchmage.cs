// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BlazingArchmage : Creature
{
	public BlazingArchmage() : base(
		Name: "Blazing Archmage",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 6,
		Text: "{Cast a spell other than \"Ignite\"}: Cast Ignite.",
		OriginalPower: 6,
		OriginalLife: 6
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}