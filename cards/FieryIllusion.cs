// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class FieryIllusion : Spell
{
	public FieryIllusion() : base(
		Name: "Fiery Illusion",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Create two 0/1 Illusions with [Decaying]."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}
}