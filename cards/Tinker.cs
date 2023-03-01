// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Tinker : Spell
{
	public Tinker() : base(
		Name: "Tinker",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Summon two 2/1 Construct tokens with [Brittle]."
		)
	{}
	// TODO: implement functionality

	public override void Init()
	{
	}
}