// scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Rebuild : Spell
{
	public Rebuild() : base(
		Name: "Rebuild",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Return target creature from any grave to the field, it gains [Brittle]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: FilterValid(GetForBoth(GetGrave), c => c.CardType == CardType.Creature), description: "Select creature to rebuild");
		MoveToField(choosingPlayer: Controller, targetPlayer: Controller, card: target);
		target.RegisterKeyword(Keyword.Brittle);
	}

	public bool CastCondition()
	{
		return HasEmpty(GetField(Controller)) && ContainsValid(cards: GetForBoth(GetGrave), isValid: card => card.CardType == CardType.Creature);
	}

}
