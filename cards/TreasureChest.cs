// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class TreasureChest : Creature
{
	public TreasureChest() : base(
		Name: "Treasure Chest",
		CardClass: PlayerClass.All,
		OriginalCost: 4,
		Text: "{Death}: Draw 2.",
		OriginalPower: 0,
		OriginalLife: 4
		)
	{ }
	// TODO: implement functionality

}