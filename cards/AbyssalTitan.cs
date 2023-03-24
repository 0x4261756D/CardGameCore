// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class AbyssalTitan : Creature
{
	public AbyssalTitan() : base(
		Name: "Abyssal Titan",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 10,
		Text: "[Collosal] +1.\nCosts 1 less for each card you discarded this turn.\n{Revelation}: Gain 3 life.",
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
		PlayerChangeLife(player: Controller, amount: 3);
	}
	private void CostReductionEffect(Card _)
	{
		this.Cost -= GetDiscardCountXTurnsAgo(player: Controller, turns: 0);
	}
}