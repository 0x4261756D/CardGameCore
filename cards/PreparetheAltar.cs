// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class PreparetheAltar : Spell
{
	public PreparetheAltar() : base(
		Name: "Prepare the Altar",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Discard 1. Gain Momentum equal to its cost.\n{Revelation}: Gain 1 Momentum. You may discard 1."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: GainDiscardEffect), referrer: this);
	}


	public void GainDiscardEffect()
	{
		PlayerChangeMomentum(player: Controller, amount: 1);
		bool doDiscard = GetDiscardable(Controller, null).Length > 0 && AskYesNo(player: Controller, question: "Discard 1?");
		if(doDiscard)
		{
			DiscardAmount(player: Controller, amount: 1);
		}
	}

	public void CastEffect()
	{
		Card target = SelectSingleCard(cards: GetDiscardable(Controller, ignore: this), player: Controller, description: "Select card to discard");
		Discard(target);
		PlayerChangeMomentum(player: Controller, amount: target.Cost);
	}

	public bool CastCondition()
	{
		return GetDiscardable(Controller, ignore: this).Length > 0;
	}


}
