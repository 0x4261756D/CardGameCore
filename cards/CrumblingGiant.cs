// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CrumblingGiant : Creature
{
	public CrumblingGiant() : base(
		Name: "Crumbling Giant",
		cardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "[Brittle]\n{Death}: Deal 5 damage to this creature's owner.",
		OriginalPower: 5,
		OriginalLife: 4
		)
	{ }
	// TODO: implement functionality

}