// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BurningPassion : Quest
{
	public BurningPassion() : base(
		Name: "Burning Passion",
		CardClass: PlayerClass.Pyromancer,
		Text: "{You cast \"Ignite\"}: Gain 1 Progress.\n{Reward}: \"Ignite\" deals +1 damage for the rest of the game.",
		ProgressGoal: 15
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}