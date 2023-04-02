// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Fireball : Spell
{
	public Fireball() : base(
		Name: "Fireball",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 2,
		Text: "{Cast}: Deal 6 damage to any target.\n{Revelation}: If your opponent controls a damaged creature, add this to your hand."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect, condition: RevelationCondition), referrer: this);
	}

	private void RevelationEffect()
	{
		MoveToHand(player: Controller, card: this);
	}

	private bool RevelationCondition()
	{
		return ContainsValid(GetFieldUsed(1 - Controller), DamagedFilter);
	}

	private bool DamagedFilter(Card card)
	{
		return card.BaseLife != card.Life;
	}

	private void CastEffect()
	{
		ChangeLifeOfAnyTarget(player: Controller, amount: -6, description: "Fireball");
	}
}