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
		return HasUsed(GetBothFieldsWhole());
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Select target");
		RegisterDeathTrigger(trigger: new TargetingTrigger(effect: ExplosionEffect), referrer: target);
	}

	private void ExplosionEffect(Card target)
	{
		ChangeLifeOfAnyTarget(player: target.Controller, amount: -((Creature)target).BaseLife, source: target);
	}
}
