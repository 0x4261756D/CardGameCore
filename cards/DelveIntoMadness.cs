// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DelveIntoMadness : Spell
{
	public DelveIntoMadness() : base(
		Name: "Delve into Madness",
		cardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Draw a card.\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

}