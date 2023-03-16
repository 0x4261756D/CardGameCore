// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class DrainSoul : Spell
{
	public DrainSoul() : base(
		Name: "Drain Soul",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Destroy 1 creature. Gain life equal to its power.\n{Discard}: Gain 3 life.\n{Revelation}: Gain 1 life."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterDiscardTrigger(trigger: new DiscardTrigger(effect: DiscardEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	private void RevelationEffect()
	{
		PlayerChangeLife(player: Controller, amount: 1);
	}

	private void DiscardEffect()
	{
		PlayerChangeLife(player: Controller, amount: 3);
	}

	private void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select creature to drain")[0];
		Destroy(target);
		PlayerChangeLife(player: Controller, amount: target.Power);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothWholeFields());
	}
}