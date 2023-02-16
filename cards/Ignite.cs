// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Ignite : Spell
{
	public Ignite() : base(
		Name: "Ignite",
		cardClass: PlayerClass.Pyromancer,
		OriginalCost: 0,
		Text: "{Cast}: Deal 1 damage any target.\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

}