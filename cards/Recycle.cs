// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Recycle : Spell
{
	public Recycle() : base(
		Name: "Recycle",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: At the end of your turn draw X where X is the number of creatures you control."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
	}

	public void CastEffect()
	{
		RegisterStateReachedTrigger(new StateReachedTrigger(effect: DrawEffect, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), this);
	}

	private void DrawEffect()
	{
		Draw(player: Controller, amount: GetFieldUsed(Controller).Length);
	}
}
