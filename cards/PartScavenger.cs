// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class PartScavenger : Creature
{
	public PartScavenger() : base(
		Name: "Part Scavenger",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Your creature dies}: Gain +2/+2.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterGenericDeathTrigger(trigger: new GenericDeathTrigger(effect: DeathEffect, condition: DeathCondition), referrer: this);
	}

	private bool DeathCondition(Card target)
	{
		return target.Controller == Controller;
	}

	private void DeathEffect(Card target)
	{
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BoostEffect, referrer: this));
	}

	private void BoostEffect(Card t)
	{
		Creature target = (Creature)t;
		target.Life += 2;
		target.Power += 2;
	}
}