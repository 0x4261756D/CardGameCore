using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class LivingBomb : Spell
{
	public LivingBomb() : base(
		Name: "Living Bomb",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Target creature gains \"{Death}: Deal X damage to this cards owner, where X is this cards attack\"."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothWholeFields());
	}

	private void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select target")[0];
		RegisterDeathTrigger(trigger: new Trigger(effect: () => ChangeLifeOfAnyTarget(player: target.Controller, amount: -target.BaseLife, source: target)), referrer: target);
	}
}