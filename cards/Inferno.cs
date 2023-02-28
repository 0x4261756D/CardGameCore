// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Inferno : Spell
{
	public Inferno() : base(
		Name: "Inferno",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Cast}: Cast \"Ignite\" on every creature on the field."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}