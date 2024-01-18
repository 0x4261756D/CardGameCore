// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class CallingtheAbyss : Spell
{
	public CallingtheAbyss() : base(
		Name: "Calling the Abyss",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 6,
		Text: "{Cast}: Pay 6 life. Create a 6/6 \"Abyssal\" Token. Deal 6 damage to any target. At the beginning of the next turn: Gain 6 Momentum. Draw 6. [Gather] 6. Discard 6."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return HasEmpty(GetField(Controller));
	}

	public void BenefitEffect()
	{
		PlayerChangeMomentum(Controller, 6);
		Draw(Controller, 6);
		_ = Gather(Controller, 6);
		DiscardAmount(Controller, 6);
	}

	public void CastEffect()
	{
		PayLife(Controller, 6);
		CreateTokenOnField(player: Controller, power: 6, life: 6, name: "Abyssal", source: this);
		ChangeLifeOfAnyTarget(player: Controller, amount: -6, source: this);
		RegisterStateReachedTrigger(new StateReachedTrigger(effect: BenefitEffect, state: State.TurnStart, influenceLocation: Location.ALL, oneshot: true), this);
	}
}
