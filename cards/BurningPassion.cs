// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BurningPassion : Quest
{
	public BurningPassion() : base(
		Name: "Burning Passion",
		CardClass: PlayerClass.Pyromancer,
		Text: "{You cast \"Ignite\"}: Gain 1 Progress.\n{Reward}: \"Ignite\" deals +1 damage for the rest of the game.",
		ProgressGoal: 15
		)
	{ }

	public override void Init()
	{
		RegisterGenericCastTrigger(trigger: new LocationBasedTargetingTrigger(effect: ProgressionEffect, condition: Condition, influenceLocation: Location.Quest), referrer: this);
	}

	public void ProgressionEffect(Card castCard)
	{
		Progress++;
	}

	public bool Condition(Card castCard)
	{
		return castCard.Controller == Controller && castCard.Name == "Ignite";
	}

	public override void Reward()
	{
		ChangeIgniteDamage(player: Controller, amount: 1);
	}
}
