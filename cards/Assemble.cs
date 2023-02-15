// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Assemble : Spell
{
	public Assemble() : base(
		Name: "Assemble",
		cardClass: PlayerClass.Artificer,
		OriginalCost: 0,
		Text: "Cast: Create a 2/1 Construct with [Brittle].\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

}