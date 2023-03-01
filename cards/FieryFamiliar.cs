using CardGameCore;
using static CardGameUtils.GameConstants;

class FieryFamiliar : Creature
{
	public FieryFamiliar() : base(
		Name: "Firery Familiar",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "Your \"Ignite\" deals +1 damage \n{Revelation:} Add this to your hand.",
		OriginalPower: 1,
		OriginalLife: 3
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}