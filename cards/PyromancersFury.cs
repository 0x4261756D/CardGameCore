// Scripted by 0x4261756D

using CardGameCore;
using static CardGameUtils.GameConstants;

class PyromancersFury : Spell
{
	private bool creatureDiedActive;
	public PyromancersFury() : base(
		Name: "Pyromancer's Fury",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Your \"Ignite\" deals +1 damage for the rest of the game. {A creature dies this turn}: Your ability is refreshed."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterGenericDeathTrigger(trigger: new CreatureTargetingTrigger(effect: RefreshEffect, condition: RefreshCondition, influenceLocation: Location.Grave), referrer: this);
	}

	private bool RefreshCondition(Creature _)
	{
		return creatureDiedActive;
	}


	public void RefreshEffect(Card _)
	{
		RefreshAbility(Controller);
	}

	public void CastEffect()
	{
		ChangeIgniteDamage(player: Controller, amount: 1);
		creatureDiedActive = true;
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: ResetEffect, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: this);
	}

	public void ResetEffect()
	{
		creatureDiedActive = false;
	}
}
