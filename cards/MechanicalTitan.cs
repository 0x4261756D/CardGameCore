// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class MechanicalTitan : Creature
{
	public MechanicalTitan() : base(
		Name: "Mechanical Titan",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 6,
		Text: "[Collossal] +1\nCosts 1 less to cast for each creature with [Brittle] that died last turn.\n{Revelation}: Gain 3 life.",
		OriginalPower: 7,
		OriginalLife: 7
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Colossal, 1);
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: CostReductionEffect, referrer: this, influenceLocation: Location.Hand));
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: GainLifeEffect), referrer: this);
	}

	private void GainLifeEffect()
	{
		PlayerChangeLife(player: Controller, amount: 3, source: this);
	}

	private void CostReductionEffect(Card target)
	{
		this.Cost -= GetBrittleDeathCountXTurnsAgo(player: Controller, turns: 1);
	}
}