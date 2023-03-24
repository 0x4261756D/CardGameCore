// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class ImmortalPhoenix : Creature
{
	public ImmortalPhoenix() : base(
		Name: "Immortal Phoenix",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Death}: Return this to your hand. For the rest of the game all \"Immortal Phoenix\" you control gain +1/+1. ",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterDeathTrigger(trigger: new Trigger(effect: DeathEffect), referrer: this);
	}

	private void DeathEffect()
	{
		MoveToHand(player: Controller, card: this);
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: PhoenixEffect, referrer: this, influenceLocation: Location.ALL));
	}

	private void PhoenixEffect(Card target)
	{
		foreach(Card card in GetFieldUsed(player: Controller))
		{
			if(card.Name == this.Name)
			{
				RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: card));
			}
		}
	}

	private void BuffEffect(Card target)
	{
		target.Life++;
		target.Power++;
	}
}