// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class PreparetheAltar : Spell
{
	public PreparetheAltar() : base(
		Name: "Prepare the Altar",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Discard 1. Gain Momentum equal to its cost.\n{Revelation}: Gain 1 Momentum. You may discard 1.",
		CanBeClassAbility: true
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: GainDiscardEffect), referrer: this);
	}


	public void GainDiscardEffect()
	{
		PlayerChangeMomentum(player: Controller, amount: 1);
		bool doDiscard = AskYesNo(player: Controller, question: "Discard 1?");
		if(doDiscard)
		{
			DiscardAmount(player: Controller, amount: 1);
		}
	}

	public void CastEffect()
	{
		Card target = SelectCards(cards: GetHand(Controller), amount: 1, player: Controller, description: "Select card to discard")[0];
		PlayerChangeMomentum(player: Controller, amount: target.Cost);
	}

	public bool CastCondition()
	{
		return GetHand(Controller).Length > 0;
	}


}