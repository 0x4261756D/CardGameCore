// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class AttentiontoDetail : Spell
{
	public AttentiontoDetail() : base(
		Name: "Attention to Detail",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Create a token copy of target creature. The copy gains [Brittle]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Select card to copy");
		Creature copy = CreateTokenCopy(player: Controller, card: target);
		copy.RegisterKeyword(Keyword.Brittle);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: copy, source: this);
	}
	public bool CastCondition()
	{
		Card?[] ownField = GetField(Controller);
		return HasEmpty(ownField) && (HasUsed(ownField) || HasUsed(GetField(1 - Controller)));
	}
}
