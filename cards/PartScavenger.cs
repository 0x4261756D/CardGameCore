// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

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
		RegisterGenericDeathTrigger(trigger: new CreatureTargetingTrigger(effect: DeathEffect, condition: DeathCondition, influenceLocation: Location.Field), referrer: this);
	}

	private bool DeathCondition(Creature target)
	{
		return target.Controller == Controller;
	}

	private void DeathEffect(Creature _)
	{
		RegisterTemporaryLingeringEffect(info: LingeringEffectInfo.Create(effect: BoostEffect, referrer: this));
	}

	private void BoostEffect(Creature target)
	{
		target.Life += 2;
		target.Power += 2;
	}
}
