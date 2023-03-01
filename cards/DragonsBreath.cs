using CardGameCore;
using static CardGameUtils.GameConstants;

class DragonsBreath : Spell
{
	public DragonsBreath() : base(
		Name: "Dragon's Breath",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Cast}: All creatures gain -3/-0 and [Decaying]."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}