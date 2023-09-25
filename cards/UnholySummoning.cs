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
		Text: "{Cast}: Discard 1. Create an X/X Horror token with where X is the discarded Card's Momentum cost.\n{Discard}: Create a 3/3 Horror Token."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: Condition), referrer: this);
		RegisterDiscardTrigger(trigger: new Trigger(effect: DiscardEffect, condition: DiscardCondition), referrer: this);
	}

	public bool DiscardCondition()
	{
		return HasEmpty(GetField(Controller));
	}

	public void DiscardEffect()
	{
		CreateTokenOnField(player: Controller, power: 3, life: 3, name: "Horror", source: this);
	}

	private bool Condition()
	{
		return GetDiscardable(Controller, ignore: this).Length > 0 && HasEmpty(GetField(Controller));
	}

	private void CastEffect()
	{
		Card target = SelectSingleCard(cards: GetDiscardable(Controller, ignore: this), player: Controller, description: "Select card to discard");
		Discard(target);
		CreateTokenOnField(player: Controller, power: target.Cost, life: target.Cost, name: "Horror", source: this);
	}
}
