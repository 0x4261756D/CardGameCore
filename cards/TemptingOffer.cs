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

	public void CastEffect(){
		Card[] target = SelectCards(cards: GetHand(Controller), amount: 2, player: Controller, description: "Select cards to discard");
		Discard(target[0]);
		Discard(target[1]);
		PlayerChangeMomentum(player: Controller, amount: 2);
	}

	public bool CastCondition(){
		return GetHand(Controller).Length > 1;
	}

}