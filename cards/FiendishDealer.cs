// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class FiendishDealer : Creature
{
	public FiendishDealer() : base(
		Name: "Fiendish Dealer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Activate}: Draw 1. You must have discarded this turn.",
		OriginalPower: 3,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		RegisterActivatedEffect(new ActivatedEffectInfo(name: "Activate", effect: ActivateEffect, condition: ActivateCondition, referrer: this));
	}

	private bool ActivateCondition()
	{
		return GetDiscardCountXTurnsAgo(player: Controller, turns: 0) > 0;
	}

	private void ActivateEffect()
	{
		Draw(player: Controller, amount: 1);
	}
}
