// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class AcolyteofMadness : Creature
{
	public AcolyteofMadness() : base(
		Name: "Acolyte of Madness",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 4,
		Text: "{You Discard}: Your opponent loses 2 life. Gain 2 life.",
		OriginalPower: 5,
		OriginalLife: 5
		)
	{ }

	public override void Init()
	{
		RegisterYouDiscardTrigger(trigger: new YouDiscardTrigger(effect: DiscardEffect), referrer: this);
	}

	public void DiscardEffect()
	{
		PlayerChangeLife(player: 1 - Controller, amount: -2);
		PlayerChangeLife(player: Controller, amount: 2);
	}
}