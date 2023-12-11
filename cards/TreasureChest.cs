// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class TreasureChest : Creature
{
	public TreasureChest() : base(
		Name: "Treasure Chest",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Death}: Draw 2.\n{Revelation}: Reveal 2.",
		OriginalPower: 0,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterDeathTrigger(trigger: new CreatureTargetingTrigger(effect: DeathEffect, influenceLocation: Location.Field), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect), referrer: this);
	}

	public void RevelationEffect()
	{
		Reveal(player: Controller, damage: 2);
	}

	public void DeathEffect(Creature _)
	{
		Draw(player: Controller, amount: 2);
	}

}
