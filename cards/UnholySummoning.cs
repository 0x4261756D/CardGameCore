// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class UnholySummoning : Spell
{
	public UnholySummoning() : base(
		Name: "Unholy Summoning",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Discard 1. Create an X/X Horror token with [Decaying] where X is the discarded Card's Momentum cost.\n{Discard}: Create a 3/2 Horror Token."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: Condition), referrer: this);
		RegisterDiscardTrigger(trigger: new DiscardTrigger(effect: DiscardEffect, condition: DiscardCondition), referrer: this);
	}

	public bool DiscardCondition()
	{
		return HasEmpty(GetField(Controller));
	}

	public void DiscardEffect()
	{
		CreateToken(player: Controller, power: 3, life: 2, name: "Horror");
	}

	private bool Condition()
	{
		return GetHand(Controller).Length > 0 && HasEmpty(GetField(Controller));
	}

	private void CastEffect()
	{
		Card target = SelectCards(cards: GetHand(Controller), amount: 1, player: Controller, description: "Select card to discard")[0];
		Discard(target);
		CreateToken(player: Controller, power: target.Cost, life: target.Cost, name: "Horror");
	}
}