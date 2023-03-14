// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EverlastingProgress : Quest
{
	public EverlastingProgress() : base(
		Name: "Everlasting Progress",
		CardClass: PlayerClass.Artificer,
		ProgressGoal: 10,
		Text: "{A creature with brittle dies}: Gain 1 progress.\n{Reward}: All creatures you control lose brittle."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

	public override void Reward()
	{
		throw new NotImplementedException();
	}

}