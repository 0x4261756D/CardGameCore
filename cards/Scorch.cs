using CardGameCore;
using static CardGameUtils.GameConstants;

class Scorch : Spell
{
	public Scorch() : base(
		Name: "Scorch",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Cast}: Target creatures attacks becomes 0. It gains [Decaying]."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}