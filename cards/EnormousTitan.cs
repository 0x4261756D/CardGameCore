// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class EnormousTitan : Creature
{
	public EnormousTitan() : base(
		Name: "Enormous Titan",
		CardClass: PlayerClass.All,
		OriginalCost: 6,
		Text: "[Colossal] +1\n{Cast}: Costs 1 less for each creature your opponent controls.\n{Revelation}: Gain 2 life.",
		OriginalPower: 7,
		OriginalLife: 7
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Colossal, 1);
		RegisterLingeringEffect(info: LingeringEffectInfo.Create(effect: CostReductionEffect, referrer: this, influenceLocation: Location.Hand));
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: GainEffect), referrer: this);
	}

	private void GainEffect()
	{
		PlayerChangeLife(player: Controller, amount: 2, source: this);
	}

	private void CostReductionEffect(Creature _)
	{
		this.Cost -= GetFieldUsed(1 - Controller).Length;
	}

}