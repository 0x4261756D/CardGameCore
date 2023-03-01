// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DeceivingTactician : Creature
{
	public DeceivingTactician() : base(
		Name: "Deceiving Tactician",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Activate}: Move target creature your opponent controls.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}