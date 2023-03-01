using CardGameCore;
using static CardGameUtils.GameConstants;

class BurningMemory : Spell
{
	public BurningMemory() : base(
		Name: "Burning Memory",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Cast}: Return target creature from a grave to your field. It gains [Decaying] and loses all other abilities."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}