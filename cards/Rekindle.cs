using CardGameCore;
using static CardGameUtils.GameConstants;

class Rekindle : Spell
{
	public Rekindle() : base(
		Name: "Rekindle",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Draw X, where X is the amount of Damage \"Ignite\" would deal \n {Revelation:} Draw 1."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}