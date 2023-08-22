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
		RegisterGenericDeathTrigger(trigger: new GenericDeathTrigger(effect: ProgressEffect, condition: ProgressCondition), referrer: this);
	}

	private bool ProgressCondition(Card destroyedCard)
	{
		return destroyedCard.Controller == Controller && ((Creature)destroyedCard).Keywords.ContainsKey(Keyword.Brittle);
	}

	private void ProgressEffect(Card destroyedCard)
	{
		Progress++;
	}

	public override void Reward()
	{
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: RewardEffect, referrer: this, influenceLocation: Location.Quest));
	}

	private void RewardEffect(Card target)
	{
		foreach(Creature card in GetFieldUsed(Controller))
		{
			if(card.Keywords.Remove(Keyword.Brittle))
			{
				RegisterTemporaryLingeringEffect(new LingeringEffectInfo(effect: BuffEffect, referrer: card));
			}
		}
	}

	private void BuffEffect(Card t)
	{
		Creature target = (Creature)t;
		target.Life += 2;
		target.Power++;
	}
}