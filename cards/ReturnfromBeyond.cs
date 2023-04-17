// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class ReturnfromBeyond : Spell
{
	public ReturnfromBeyond() : base(
		Name: "Return from Beyond",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Return target creature from the grave to your field. Shuffle X cards from your hand into the deck, X is half the creatures Cost (rounded up)."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		int maxCost = GetMaxCost();
		Card[] possibleTargets = FilterValid(cards: GetGrave(Controller), card => card.CardType == CardType.Creature && card.Cost <= maxCost);
		Card target = SelectCards(player: Controller, cards: possibleTargets, amount: 1, description: "Select card to return")[0];
		ReturnCardsToDeck(SelectCards(player: Controller, cards: GetHand(Controller), amount: (target.Cost + (target.Cost & 1)) / 2, description: "Select cards to return to deck"));
		MoveToField(choosingPlayer: Controller, targetPlayer: Controller, card: target);
	}

	int GetMaxCost()
	{
		return FilterValid(GetHand(Controller), (x) => x.uid != this.uid).Length * 2;
	}

	public bool CastCondition()
	{
		int maxCost = GetMaxCost();
		return ContainsValid(GetGrave(Controller), card => card.CardType == CardType.Creature && card.Cost <= maxCost);
	}
}