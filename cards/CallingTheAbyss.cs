// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CallingtheAbyss : Spell
{
	public CallingtheAbyss() : base(
		Name: "Calling the Abyss",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 6,
		Text: "{Cast}: Pay 6 life. Discard 6 cards. At the beginning of the next turn: Gain 6 Momentum. Draw 6 cards. [Gather] 6."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}