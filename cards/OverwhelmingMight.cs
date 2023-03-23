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
		Text: "{Cast}: Deal 2 damage to target creature. You may discard 1: Recast this.\n{Discard}: Deal 1 damage to any target."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	private void RevelationEffect()
	{
		ChangeLifeOfAnyTarget(player: Controller, amount: -1, "Damage");
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothWholeFields());
	}

	private void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select target to damage")[0];
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: DamageEffect, referrer: target));
		if(GetDiscardable(Controller).Length > 0 && AskYesNo(player: Controller, question: "Discard a card to recast?"))
		{
			Cast(player: Controller, card: this);
		}
	}

	private void DamageEffect(Card target)
	{
		target.Life -= 2;
	}
}