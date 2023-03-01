using CardGameCore;
using static CardGameUtils.GameConstants;

class BurningImp : Creature
{
	public BurningImp() : base(
		Name: "Burning Imp",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 2,
		Text: "Whenever you cast \"Ignite\" gain +2/+2.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}