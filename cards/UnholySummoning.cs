// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class UnholySummoning : Spell
{
	public UnholySummoning() : base(
		Name: "Unholy Summoning",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Discard 1. Create 1 X/X Horror token where X is the discarded Card's Momentum cost.\n{Discard}: Create a 3/2 Horror Token."
		)
	{ }
	// TODO: implement discard trigger

	public override void Init()
	{
		RegisterCastTrigger(effect: CastEffect, condition: Condition, referrer: this);
	}

	private bool Condition()
	{
		return GetHand(Controller).Length > 0 && GetField(Controller).Any(x => x == null);
	}

	private void CastEffect()
	{
		Card target = SelectCards(cards: GetHand(Controller), amount: 1, player: Controller, description: "Select card to discard")[0];
		Discard(target);
		CreateToken(player: Controller, power: target.Cost, life: target.Cost, name: "Horror");
	}
}