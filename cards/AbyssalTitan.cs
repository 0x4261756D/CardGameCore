// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class AbyssalTitan : Creature
{
	public AbyssalTitan() : base(
		Name: "Abyssal Titan",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 10,
		Text: "[Collosal] +1.\nCosts 1 less for each card you discarded this turn.\n{Revelation}: Gain 5 life.",
		OriginalPower: 8,
		OriginalLife: 8
		)
	{ }
	// TODO: revelation

	public override void Init()
	{
		RegisterKeyword(Keyword.Colossal, 1);
		RegisterLingeringEffect(info: new LingeringEffectInfo(CostReductionEffect, this, Location.Hand));
	}

	private void CostReductionEffect()
	{
		this.Cost -= GetDiscardCountThisTurn(player: Controller);
	}
}