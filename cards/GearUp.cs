// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class GearUp : Spell
{
	public GearUp() : base(
		Name: "Gear Up",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 3,
		Text: "{Cast}: Target creature you control gets +2/+2 and [Brittle] this turn.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}