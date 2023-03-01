using CardGameCore;
using static CardGameUtils.GameConstants;

class Equalizer : Spell
{
	public Equalizer() : base(
		Name: "Equalizer",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Target creatures attack and life become the greater of the two values."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}