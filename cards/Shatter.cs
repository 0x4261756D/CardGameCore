using CardGameCore;
using static CardGameUtils.GameConstants;

class Shatter : Spell
{
	public Shatter() : base(
		Name: "Shatter",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Sacrifice a creature. Deal damage to your opponent equal to its attack."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}