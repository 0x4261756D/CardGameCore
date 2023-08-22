// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ImmortalPhoenix : Creature
{
	public ImmortalPhoenix() : base(
		Name: "Immortal Phoenix",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 2,
		Text: "{Death}: Return this to your hand. For the rest of the game all your \"Immortal Phoenix\" gain +1/+1. ",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterDeathTrigger(trigger: new TargetingTrigger(effect: DeathEffect), referrer: this);
	}

	private void DeathEffect(Card target)
	{
		MoveToHand(player: Controller, card: target);
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: PhoenixEffect, referrer: target, influenceLocation: Location.ALL));
	}

	private void PhoenixEffect(Card target)
	{
		foreach(Card card in GetFieldUsed(player: Controller))
		{
			if(card.Name == this.Name)
			{
				BuffEffect(card);
			}
		}
	}

	private void BuffEffect(Card t)
	{
		Creature target = (Creature)t;
		target.Life++;
		target.Power++;
	}
}