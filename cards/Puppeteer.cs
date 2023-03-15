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
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterActivatedEffect(info: new ActivatedEffectInfo(name: "Move one of your creatures", effect: MoveEffect, condition: MoveCondition, referrer: this));
	}

	private bool MoveCondition(){
		return HasUsed(GetField(Controller));
	}

	private void MoveEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetFieldUsed(player: Controller), amount: 1, description: "Select creature to move")[0];
		Move(card: target, zone: SelectZone(choosingPlayer: Controller, targetPlayer: Controller));
	}

}