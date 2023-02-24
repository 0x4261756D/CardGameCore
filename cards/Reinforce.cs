// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Reinforce : Spell
{
	public Reinforce() : base(
		Name: "Reinforce",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: Target creature gains +0/+2 it loses [Brittle].\n{Revelation}: Gain 2 life."
		)
	{ }
	// TODO: implement functionality

}