// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CrumblingGiant : Creature
{
	public CrumblingGiant() : base(
		Name: "Crumbling Giant",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "[Brittle]\n{Death}: Deal 5 damage to this creature's owner.",
		OriginalPower: 7,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Brittle);
		RegisterDeathTrigger(trigger: new Trigger(DeathEffect), referrer: this);
	}

	public void DeathEffect()
	{
		PlayerChangeLife(player: Controller, amount: -5, source: this);
	}

}