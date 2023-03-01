using CardGameCore;
using static CardGameUtils.GameConstants;

class FlamingBeast : Creature
{
	public FlamingBeast() : base(
		Name: "Flaming Beast",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Attack:} Cast \"Ignite\"",
		OriginalPower: 3,
		OriginalLife: 6
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}