// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CursedbyGreed : Spell
{
	public CursedbyGreed() : base(
		Name: "Cursed by Greed",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Pay 3 life. Draw 1.\n{Discard}: Draw 2."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}