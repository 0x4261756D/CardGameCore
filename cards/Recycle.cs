// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Recycle : Spell
{
	public Recycle() : base(
		Name: "Recycle",
		cardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: At the end of the turn draw a card for each of your creatures that died this turn."
		)
	{ }
	// TODO: implement functionality

}