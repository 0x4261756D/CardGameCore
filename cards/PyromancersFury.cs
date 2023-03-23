// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class PyromancersFury : Spell
{
	private bool discardTriggerActive = false;
	public PyromancersFury() : base(
		Name: "Pyromancer's Fury",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Your \"Ignite\" deals +1 damage for the rest of the game. {A creature dies this turn}: Your ability is refreshed."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: LingeringEffect, referrer: this, influenceLocation: Location.ALL));
	}

	public void LingeringEffect(Card target)
	{
		if(!discardTriggerActive)
		{
			return;
		}
		RefreshAbility(Controller);
	}

	public void CastEffect()
	{
		ChangeIgniteDamage(player: Controller, amount: 1);
		discardTriggerActive = true;
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: ResetEffect, referrer: this,  influenceLocation: Location.ALL));
	}

	public void ResetEffect(Card _)
	{
		discardTriggerActive = false;
	}
}