// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ReturnfromBeyond : Spell
{
	public ReturnfromBeyond() : base(
		Name: "Return from Beyond",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Return target creature from the grave to your field. Shuffle X cards from your hand into the deck, X is half the creatures Cost (rounded up)."
		)
	{ }
	// TODO: implement functionality

}