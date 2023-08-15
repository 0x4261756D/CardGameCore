// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class RottenMummy : Creature
{
	public RottenMummy() : base(
		Name: "Rotten Mummy",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "[Decaying]\n{Death}: Discard 1.",
		OriginalLife: 7,
		OriginalPower: 5
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Decaying);
		RegisterDeathTrigger(trigger: new TargetingTrigger(effect: DeathEffect, condition: DeathCondition), referrer: this);
	}

	public void DeathEffect(Card _)
	{
		DiscardAmount(player: Controller, amount: 1);
	}

	public bool DeathCondition(Card _)
	{
		return GetDiscardable(Controller, ignore: null).Length > 0;
	}
}