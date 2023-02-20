// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CursedByGreed : Spell
{
	public CursedByGreed() : base(
		Name: "Cursed by Greed",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Pay 3 life. Draw 1 Card.\n{Discard}: Draw 2 Cards."
		)
	{ }
	// TODO: implement functionality

}