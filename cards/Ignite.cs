// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Ignite : Spell
{
	public Ignite() : base(
		Name: "Ignite",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 0,
		Text: "{Cast}: Deal 1 damage any target.\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: IgniteEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	public void IgniteEffect()
	{
		PlayerChangeLife(player: 1 - Controller, amount: -GetIgniteDamage(Controller));
	}

	public void RevelationEffect()
	{
		Cast(card: this, player: Controller);
	}

}