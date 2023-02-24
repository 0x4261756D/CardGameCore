// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ReplicatingGolem : Creature
{
	public ReplicatingGolem() : base(
		Name: "Replicating Golem",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 4,
		Text: "{Attack}: Create a token copy of this card with [Brittle].",
		OriginalPower: 1,
		OriginalLife: 1
		)
	{ }
	// TODO: implement functionality

}