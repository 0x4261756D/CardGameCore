// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class TitanSlayer : Creature
{
	public TitanSlayer() : base(
		Name: "Titan Slayer",
		CardClass: PlayerClass.All,
		OriginalCost: 3,
		Text: "{Cast}: This creature's power becomes that of target opponent's creature. The target gains [Immovable].",
		OriginalPower: 0,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return HasUsed(GetField(1 - Controller));
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetFieldUsed(1 - Controller), description: "Select target");
		CreatureChangePower(target: this, amount: target.Power, source: this);
		target.RegisterKeyword(Keyword.Immovable);
	}
}
