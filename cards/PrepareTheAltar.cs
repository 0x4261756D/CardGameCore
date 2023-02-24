// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class PreparetheAltar : Spell
{
	public PreparetheAltar() : base(
		Name: "Prepare the Altar",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Discard 1 card. Gain Momentum equal to its cost.\n{Revelation}: Gain 1 Momentum. You may discard 1 card.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

}