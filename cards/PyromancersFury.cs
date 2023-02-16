// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class PyromancersFury : Spell
{
	public PyromancersFury() : base(
		Name: "Pyromancer's Fury",
		cardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "Your ability deals +1 damage for the rest of the game. {A creature dies this turn}: Your ability is refreshed."
		)
	{}
	// TODO: implement functionality
}