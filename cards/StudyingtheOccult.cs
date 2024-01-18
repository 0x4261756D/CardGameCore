// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class StudyingtheOccult : Quest
{
	public StudyingtheOccult() : base(
		Name: "Studying the Occult",
		CardClass: PlayerClass.Cultist,
		Text: "{You discard a card}: Gain 1 Progress.\n{Reward}: This card gains \"{You discard a card}: Your opponent takes 2 damage.\".",//All your cards gain \"{Discard}: Your opponent takes 2 damage.\"",
		ProgressGoal: 10
		)
	{ }

	public override void Init()
	{
		RegisterYouDiscardTrigger(trigger: new LocationBasedTrigger(effect: ProgressEffect, influenceLocation: Location.Quest), referrer: this);
	}

	public void ProgressEffect()
	{
		Progress++;
	}

	public override void Reward()
	{
		// NOTE: This does not match the behaviour of the old text, it registers the damage to itself instead of the discarded card.
		RegisterYouDiscardTrigger(trigger: new LocationBasedTrigger(effect: () => PlayerChangeLife(player: 1 - Controller, amount: -2, source: this), influenceLocation: Location.Quest), referrer: this);
	}
}
