using CardGameCore;
using static CardGameUtils.GameConstants;

class CommandToAttack : Spell
{
	public CommandToAttack() : base(
		Name: "Command to Attack",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 4,
		Text: "{Cast}: For each of your opponents creatures create a 2/1 Construct token with [Brittle] and [Decaying]."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}