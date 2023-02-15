// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class PrepareTheAltar : Spell
{
	public PrepareTheAltar() : base(
		Name: "Prepare the Altar",
		cardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Discard a card. Gain Momentum equal to its cost.\n{Revelation}: Gain 1 Momentum. You may discard a card.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

}