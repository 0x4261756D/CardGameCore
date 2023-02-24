// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DrainSoul : Spell
{
	public DrainSoul() : base(
		Name: "Drain Soul",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Sacrifice 1 Creature. Gain life equal to its attack.\n{Discard}: Gain 3 life.\n{Revelation}: Gain 1 life."
		)
	{ }
	// TODO: implement functionality

}