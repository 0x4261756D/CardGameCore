// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class RebornSoul : Creature
{
	public RebornSoul() : base(
		Name: "Reborn Soul",
		CardClass: PlayerClass.Cultist,
		Text: "{Discard}: Cast this.\n{Revelation}: You may discard 1, if you do, create a 1/1 Weaker Soul token.",
		OriginalCost: 4,
		OriginalLife: 2,
		OriginalPower: 3
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}
}