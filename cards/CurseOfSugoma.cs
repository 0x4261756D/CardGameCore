// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CurseofSugoma : Spell
{
	public CurseofSugoma() : base(
		Name: "Curse of Sugoma",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Target creature gains +2/-1.\n{Discard}: Target creature gains decaying."
		)
	{ }
	// TODO: implement functionality

}