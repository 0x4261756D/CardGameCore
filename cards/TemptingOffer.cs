// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class TemptingOffer : Spell
{
	public TemptingOffer() : base(
		Name: "Tempting Offer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Discard 2. Gain 2 Momentum."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		DiscardAmount(player: Controller, amount: 2);
		PlayerChangeMomentum(player: Controller, amount: 2);
	}

	public bool CastCondition()
	{
		return GetHand(Controller).Length > 1;
	}

}