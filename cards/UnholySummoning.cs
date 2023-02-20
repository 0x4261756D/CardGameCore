// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class UnholySummoning : Spell
{
	public UnholySummoning() : base(
		Name: "Unholy Summoning",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Discard 1. Create 1 X/X Horror token where X is the discarded Card's Momentum cost.\n{Discard}: Create a 3/2 Horror Token."
		)
	{ }
	// TODO: implement functionality

}