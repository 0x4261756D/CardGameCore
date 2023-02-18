// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BurningTitan : Creature
{
	public BurningTitan() : base(
		Name: "Burning Titan",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 10,
		Text: "{Cast}: [Collosal] 1. Costs 1 less for each damage you dealt with spells last turn.\n{Revelation}: Gain 5 life.",
		OriginalPower: 8,
		OriginalLife: 8
		)
	{ }
	// TODO: implement functionality

}