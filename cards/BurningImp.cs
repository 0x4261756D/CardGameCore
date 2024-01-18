// scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BurningImp : Creature
{
	public BurningImp() : base(
		Name: "Burning Imp",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "Whenever you cast \"Ignite\" gain +2/+2.",
		OriginalPower: 1,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		RegisterGenericCastTrigger(trigger: new LocationBasedTargetingTrigger(condition: BuffCondition, effect: BuffEffect, influenceLocation: Location.Field), referrer: this);
	}

	public bool BuffCondition(Card castCard)
	{
		return castCard.Controller == Controller && castCard.Name == "Ignite";
	}

	public void BuffEffect(Card castCard)
	{
		RegisterTemporaryLingeringEffect(info: LingeringEffectInfo.Create(effect: Buff, referrer: this));
	}

	public void Buff(Creature target)
	{
		target.Life += 2;
		target.Power += 2;
	}

}
