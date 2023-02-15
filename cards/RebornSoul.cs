// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class RebornSoul : Creature
{
	public RebornSoul() : base(
		Name: "Reborn Soul",
		cardClass: PlayerClass.Cultist,
		Text: "{Discard}: Summon this.\n{Revelation}: You may discard a card, if you do, summon a 1/1 \"Weaker Soul\".",
		OriginalCost: 4,
		OriginalLife: 2,
		OriginalPower: 3
		)
	{}
	// TODO: implement functionality
}