// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Recycle : Spell
{
	public Recycle() : base(
		Name: "Recycle",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{End of turn}: Draw 1 for each of your creatures that died this turn."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}