// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class MechanicalTitan : Creature
{
	public MechanicalTitan() : base(
		Name: "Mechanical Titan",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 10,
		Text: "[Collosal] +1\nCosts 1 less to cast for each creature with [Brittle] that died last turn.\n{Revelation}: Gain 5 life.",
		OriginalPower: 8,
		OriginalLife: 8
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
		PlayerChangeLife(player: Controller, amount: 5);
	}

	private void CostReductionEffect(Card target)
	{
		this.Cost -= GetBrittleDeathCountXTurnsAgo(player: Controller, turns: 1);
	}
}