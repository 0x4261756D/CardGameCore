// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ReconstructingGolem : Creature
{
	public ReconstructingGolem() : base(
		Name: "Reconstructing Golem",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Death}: Return this to your hand.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterDeathTrigger(trigger: new Trigger(effect: DeathEffect), referrer: this);
	}

	public void DeathEffect()
	{
		MoveToHand(player: Controller, card: this);
	}

}