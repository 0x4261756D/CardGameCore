using CardGameCore;
using static CardGameUtils.GameConstants;

class Rebuild : Spell
{
	public Rebuild() : base(
		Name: "Rebuild",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Return target creature from any grave to the field it gains [Brittle]."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}