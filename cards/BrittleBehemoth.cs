// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BrittleBehemoth : Creature
{
	public BrittleBehemoth() : base(
		Name: "Brittle Behemoth",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 4,
		Text: "[Brittle]\n{Victorious}: Lose Brittle this turn.\n{Revelation}: Target creature with less than 7 power can't move this turn.",
		OriginalPower: 6,
		OriginalLife: 4
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}