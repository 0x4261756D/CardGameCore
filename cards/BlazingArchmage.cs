// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class BlazingArchmage : Creature
{
	public BlazingArchmage() : base(
		Name: "Blazing Archmage",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 6,
		Text: "{Cast a spell other than \"Ignite\"}: Cast Ignite.",
		OriginalPower: 3,
		OriginalLife: 9
		)
	{ }

	public override void Init()
	{
		RegisterGenericCastTrigger(trigger: new LocationBasedTargetingTrigger(effect: CastIgniteEffect, condition: CastIgniteCondition, influenceLocation: Location.Field), referrer: this);
	}

	public void CastIgniteEffect(Card target)
	{
		Cast(Controller, new Ignite() { BaseController = this.Controller, Controller = this.Controller });
	}

	public bool CastIgniteCondition(Card target)
	{
		return target.Controller == Controller && target.CardType == CardType.Spell && target.Name != "Ignite";
	}
}
