// Scripted by Dotlof
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Pupeteer : Creature
{
	public Pupeteer() : base(
		Name: "Pupeteer",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Activate}: Move target creature you control.",
		OriginalPower: 3,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterActivatedEffect(info: new ActivatedEffectInfo(name: "Move one of your creatures", effect: MoveEffect, condition: MoveCondition, referrer: this));
	}

	private bool MoveCondition()
	{
		return HasUsed(GetField(Controller));
	}

	private void MoveEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetFieldUsed(player: Controller), description: "Select creature to move");
		Move(card: target, zone: SelectZone(choosingPlayer: Controller, targetPlayer: Controller));
	}

}
