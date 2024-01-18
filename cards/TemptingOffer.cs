// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class TemptingOffer : Spell
{
	public TemptingOffer() : base(
		Name: "Tempting Offer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Discard 2. Gain 2 Momentum. Draw 2."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		DiscardAmount(player: Controller, amount: 2);
		PlayerChangeMomentum(player: Controller, amount: 2);
		Draw(player: Controller, amount: 2);
	}

	public bool CastCondition()
	{
		return GetDiscardable(Controller, ignore: this).Length > 1;
	}

}
