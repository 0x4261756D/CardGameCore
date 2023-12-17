// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Reforge : Spell
{
	public Reforge() : base(
		Name: "Reforge",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: Return your target non-token creature to your hand. Create a token copy of it."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, FilterValid(cards: GetFieldUsed(player: Controller), isValid: (creature) => !creature.Keywords.ContainsKey(Keyword.Token)), description: "Select card to reforge");
		Creature token = CreateTokenCopy(player: Controller, card: target);
		MoveToHand(player: Controller, card: target);
		MoveToField(choosingPlayer: Controller, targetPlayer: Controller, card: token, source: this);
	}

	private bool CastCondition()
	{
		return ContainsValid(cards: GetFieldUsed(Controller), isValid: (card) => !card.Keywords.ContainsKey(Keyword.Token));
	}
}
