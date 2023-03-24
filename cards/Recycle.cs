// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Recycle : Spell
{
	public Recycle() : base(
		Name: "Recycle",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{End of turn}: Draw 1 for each of your creatures that died this turn."
		)
	{ }

	public override void Init()
	{
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: StateReachedEffect, state: State.TurnEnd, influenceLocation: Location.ALL), referrer: this);
	}

	public void StateReachedEffect()
	{
		Draw(player: Controller, amount: GetDeathCountXTurnsAgo(player: Controller, turns: 0));
	}

}