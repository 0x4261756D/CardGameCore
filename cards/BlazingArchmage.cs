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
		OriginalPower: 6,
		OriginalLife: 6
		)
	{ }

	public override void Init()
	{
		RegisterGenericCastTrigger(trigger: new GenericCastTrigger(effect: CastIgniteEffect, condition: CastIgniteCondition), referrer: this);
	}

	public void CastIgniteEffect(Card target)
	{
		Cast(Controller, new Ignite());
	}

	public bool CastIgniteCondition(Card target)
	{
		return target.Name != "Ignite";
	}
}