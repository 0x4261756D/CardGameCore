// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class AcolyteOfMadness : Creature
{
	public AcolyteOfMadness() : base(
		Name: "Acolyte of Madness",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 4,
		Text: "{You Discard}: Your opponent loses 2 life. Gain 2 life.",
		OriginalPower: 5,
		OriginalLife: 5
		)
	{ }
	// TODO: implement functionality

}