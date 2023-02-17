// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class StudyingTheOccult : Quest
{
	public StudyingTheOccult() : base(
		Name: "Studying the Occult",
		CardClass: PlayerClass.Cultist,
		Text: "{You discard a card}: Gain 1 Progress.\n{Reward}: All your cards gain \"{Discard} Your opponent takes 2 damage.\".",
		ProgressGoal: 10
		)
	{ }
	// TODO: implement functionality

}