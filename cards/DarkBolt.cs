// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DarkBolt : Spell
{
	public DarkBolt() : base(
		Name: "Dark Bolt",
		cardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Deal 3 damage any target.\n{Revelation}: Cast this."
		)
	{ }
	// TODO: implement functionality

}