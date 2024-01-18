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
		RegisterDeathTrigger(trigger: new CreatureTargetingTrigger(effect: DeathEffect, influenceLocation: Location.Grave), referrer: this);
	}

	private void DeathEffect(Creature target)
	{
		MoveToHand(player: Controller, card: target);
		RegisterLingeringEffect(info: LingeringEffectInfo.Create(effect: PhoenixEffect, referrer: target, influenceLocation: Location.ALL));
	}

	private void PhoenixEffect(Creature target)
	{
		foreach(Creature card in GetFieldUsed(player: Controller))
		{
			if(card.Name == target.Name)
			{
				BuffEffect(card);
			}
		}
	}

	private static void BuffEffect(Creature target)
	{
		target.Life++;
		target.Power++;
	}
}
