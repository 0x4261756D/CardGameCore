// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class RottenMummy : Creature
{
	public RottenMummy() : base(
		Name: "Rotten Mummy",
		cardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "[Decaying]\n{Death}: Discard 1.",
		OriginalLife: 4,
		OriginalPower: 3
		)
	{}
	// TODO: implement functionality
}