// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CallingtheAbyss : Spell
{
	public CallingtheAbyss() : base(
		Name: "Calling the Abyss",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 6,
		Text: "{Cast}: Pay 6 life. Discard 6. At the beginning of the next turn: Gain 6 Momentum. Draw 6. [Gather] 6."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void BenefitEffect()
	{
		PlayerChangeMomentum(player: Controller, amount: 6);
		Draw(player: Controller, amount: 6);
		Gather(player: Controller, amount: 6);
	}

	public void CastEffect()
	{
		PayLife(player: Controller, amount: 6);
		DiscardAmount(player: Controller, amount: 6);
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: BenefitEffect, state: State.TurnStart, influenceLocation: Location.ALL, oneshot: true), referrer: this);
	}

	public bool CastCondition()
	{
		return GetDiscardable(Controller).Length >= 6;
	}

}