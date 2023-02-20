// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EnormousTitan : Creature
{
	public EnormousTitan() : base(
		Name: "Enormous Titan",
		CardClass: PlayerClass.All,
		OriginalCost: 10,
		Text: "[Collosal] +1\n{Cast}: Costs 1 less for each creature your opponent controls.\n{Revelation}: Gain 3 life.",
		OriginalPower: 8,
		OriginalLife: 8
		)
	{ }
	// TODO: implement functionality

}