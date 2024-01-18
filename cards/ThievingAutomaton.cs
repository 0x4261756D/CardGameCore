// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ThievingAutomaton : Creature
{
	public ThievingAutomaton() : base(
		Name: "Thieving Automaton",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Deals damage to your opponent}: Draw 1.",
		OriginalPower: 2,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterDealsDamageTrigger(trigger: new Trigger(effect: () => Draw(player: Controller, amount: 1)), referrer: this);
	}

}
