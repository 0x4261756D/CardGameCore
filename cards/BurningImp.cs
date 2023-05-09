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
		RegisterGenericCastTrigger(trigger: new GenericCastTrigger(condition: BuffCondition, effect: BuffEffect), referrer: this);
	}

	public bool BuffCondition(Card castCard)
	{
		return castCard.Controller == Controller && castCard.Name == "Ignite";
	}

	public void BuffEffect(Card castCard)
	{
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: Buff, referrer: this));
	}

	public void Buff(Card _)
	{
		this.Life += 2;
		this.Power += 2;
	}

}