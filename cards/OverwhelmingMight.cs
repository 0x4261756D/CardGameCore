// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class OverwhelmingMight : Spell
{
	public OverwhelmingMight() : base(
		Name: "Overwhelming Might",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Deal 2 damage to target creature. You may discard 1: Recast this.\n{Discard}: Deal 1 damage to any target."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}
}