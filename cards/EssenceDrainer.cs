// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EssenceDrainer : Creature
{
	public EssenceDrainer() : base(
		Name: "Essence Drainer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: This gains +2/+2 for each card you discarded this turn.",
		OriginalPower: 1,
		OriginalLife: 1
		)
	{ }
	// TODO: implement functionality

}