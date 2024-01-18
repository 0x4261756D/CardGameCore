// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EverlastingProgress : Quest
{
	public EverlastingProgress() : base(
		Name: "Everlasting Progress",
		CardClass: PlayerClass.Artificer,
		ProgressGoal: 10,
		Text: "{Your creature with [Brittle] dies}: Gain 1 progress.\n{Reward}: All your creatures with [Brittle] lose [Brittle] and gain +1/+2."
		)
	{ }

	public override void Init()
	{
		RegisterGenericDeathTrigger(trigger: new CreatureTargetingTrigger(effect: ProgressEffect, condition: ProgressCondition, influenceLocation: Location.Quest), referrer: this);
	}

	private bool ProgressCondition(Creature destroyedCard)
	{
		return destroyedCard.Controller == Controller && destroyedCard.Keywords.ContainsKey(Keyword.Brittle);
	}

	private void ProgressEffect(Card _)
	{
		Progress++;
	}

	public override void Reward()
	{
		RegisterLingeringEffect(info: LingeringEffectInfo.Create(effect: RewardEffect, referrer: this, influenceLocation: Location.Quest));
	}

	private void RewardEffect(Card _)
	{
		foreach(Creature card in GetFieldUsed(Controller))
		{
			if(card.Keywords.Remove(Keyword.Brittle))
			{
				RegisterTemporaryLingeringEffect(LingeringEffectInfo.Create(effect: BuffEffect, referrer: card));
			}
		}
	}

	private void BuffEffect(Creature target)
	{
		target.Life += 2;
		target.Power++;
	}
}
