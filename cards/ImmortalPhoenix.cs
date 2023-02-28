// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ImmortalPhoenix : Creature
{
	public ImmortalPhoenix() : base(
		Name: "Immortal Phoenix",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Death}: Return this to your hand. For the rest of the game all \"Immortal Phoenix\" you control gain +1/+1. ",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}