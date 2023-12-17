// Scripted by Dotlof
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class PreparetheAltar : Spell
{
	public PreparetheAltar() : base(
		Name: "Prepare the Altar",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Discard 1. Lose life equal to its power. Gain Momentum equal to its cost during the next turn.\n{Revelation}: Gain 1 Momentum. You may discard 1."
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
		PlayerChangeLife(player: Controller, amount: -target.Cost, source: this);
		Discard(target);
		RegisterStateReachedTrigger(new StateReachedTrigger(
			effect: () => PlayerChangeMomentum(player: Controller, amount: target.Cost),
			state: State.TurnStart,
			influenceLocation: Location.ALL, oneshot: true), referrer: this);

	}

	public bool CastCondition()
	{
		return GetDiscardable(Controller, ignore: this).Length > 0;
	}


}
