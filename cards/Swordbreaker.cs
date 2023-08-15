// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Swordbreaker : Creature
{
	public Swordbreaker() : base(
		Name: "Swordbreaker",
		CardClass: PlayerClass.All,
		OriginalCost: 3,
		Text: "{Cast}: Target creature gains -3/-0.",
		OriginalPower: 2,
		OriginalLife: 4
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetForBoth(GetFieldUsed), description: "Target creature to weaken");
		CreatureChangePower(target: target, amount: -3, source: this);
	}

	public bool CastCondition()
	{
		return HasUsed(GetField(Controller));
	}
}
