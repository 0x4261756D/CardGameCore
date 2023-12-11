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
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothFieldsWhole());
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Select target");
		RegisterDeathTrigger(trigger: new CreatureTargetingTrigger(effect: ExplosionEffect, influenceLocation: Location.Grave), referrer: target);
	}

	private static void ExplosionEffect(Creature target)
	{
		ChangeLifeOfAnyTarget(player: target.Controller, amount: -target.BaseLife, source: target);
	}
}
