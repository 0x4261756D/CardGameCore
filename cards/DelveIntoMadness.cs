// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DelveintoMadness : Spell
{
	public DelveintoMadness() : base(
		Name: "Delve into Madness",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Draw 1 card.\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

}