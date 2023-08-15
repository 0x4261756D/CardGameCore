// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CourageousKnight : Creature
{
	public CourageousKnight() : base(
		Name: "Courageous Knight",
		CardClass: PlayerClass.All,
		OriginalCost: 3,
		Text: "{A creature is cast}: Gain +1/+0.",
		OriginalPower: 0,
		OriginalLife: 4
		)
	{ }

	public override void Init()
	{
		RegisterGenericCastTrigger(trigger: new GenericCastTrigger(effect: BuffEffect), referrer: this);
	}

	public void BuffEffect(Card castCard)
	{
		if(castCard.CardType == CardType.Creature)
		{
			CreatureChangePower(target: this, amount: 1, source: this);
		}
	}

}