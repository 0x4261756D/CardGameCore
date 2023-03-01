// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DarkBolt : Spell
{
	public DarkBolt() : base(
		Name: "Dark Bolt",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Deal 3 damage any target.\n{Discard}: Cast this."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}