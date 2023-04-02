// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class DeceivingTactician : Creature
{
	public DeceivingTactician() : base(
		Name: "Deceiving Tactician",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Activate}: Move target creature your opponent controls.",
		OriginalPower: 3,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterActivatedEffect(info: new ActivatedEffectInfo(name: "Move opponent's creature", effect: MoveEffect, condition: MoveCondition, referrer: this));
	}

	private bool MoveCondition()
	{
		return HasUsed(GetField(1 - Controller));
	}

	private void MoveEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetFieldUsed(player: 1 - Controller), amount: 1, description: "Select creature to move")[0];
		Move(card: target, zone: SelectZone(choosingPlayer: Controller, targetPlayer: 1 - Controller));
	}
}