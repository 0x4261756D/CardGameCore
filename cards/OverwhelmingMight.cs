// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class OverwhelmingMight : Spell
{
	public OverwhelmingMight() : base(
		Name: "Overwhelming Might",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Deal 2 damage to target creature. You may discard 1: Recast this.\n{Discard}: Deal 2 damage to any target."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterDiscardTrigger(trigger: new DiscardTrigger(effect: RevelationEffect), referrer: this);
	}

	private void RevelationEffect()
	{
		ChangeLifeOfAnyTarget(player: Controller, amount: -2, description: "Damage", source: this);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothWholeFields());
	}

	private void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetForBoth(GetFieldUsed), description: "Select target to damage");
		CreatureChangeLife(target, amount: 2, source: this);
		if(GetDiscardable(Controller, ignore: this).Length > 0 && GetForBoth(GetFieldUsed).Length > 0 && AskYesNo(player: Controller, question: "Discard a card to recast?"))
		{
			DiscardAmount(player: Controller, amount: 1);
			Cast(player: Controller, card: this);
		}
	}
}
