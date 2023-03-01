// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class MechanicalTitan : Creature
{
	public MechanicalTitan() : base(
		Name: "Mechanical Titan",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 10,
		Text: "[Collosal] +1\n{Cast}: Costs 1 less to cast for each creature with [Brittle] that died last turn.\n{Revelation}: Gain 5 life.",
		OriginalPower: 8,
		OriginalLife: 8
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}