// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class EnormousTitan : Creature
{
	public EnormousTitan() : base(
		Name: "Enormous Titan",
		CardClass: PlayerClass.All,
		OriginalCost: 10,
		Text: "[Collosal] +1\n{Cast}: Costs 1 less for each creature your opponent controls.\n{Revelation}: Gain 2 life.",
		OriginalPower: 8,
		OriginalLife: 8
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Colossal, 1);
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: CostReductionEffect, referrer: this, influenceLocation: Location.Hand));
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: GainEffect), referrer: this);
	}

	private void GainEffect()
	{
		PlayerChangeLife(player: Controller, amount: 2);
	}

	private void CostReductionEffect(Card _)
	{
		this.Cost -= GetFieldUsed(1 - Controller).Length;
	}

}