// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class NovicePyromancer : Creature
{
	public NovicePyromancer() : base(
		Name: "Novice Pyromancer",
		cardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{A creature dies}: Deal 1 damage to target creature or player.",
		OriginalLife: 1,
		OriginalPower: 2
		)
	{}
	// TODO: implement functionality
}