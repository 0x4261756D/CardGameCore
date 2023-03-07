// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class AttentiontoDetail : Spell
{
	public AttentiontoDetail() : base(
		Name: "Attention to Detail",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 2,
		Text: "{Cast}: Create a token copy of target creature. The copy gains [Brittle]."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetFieldUsed(Controller), amount: 1, description: "Select card to copy")[0];
		Card copy = CreateTokenCopy(player: Controller, card: target);
		copy.RegisterKeyword(Keyword.Brittle);
	}
	public bool CastCondition()
	{
		Card?[] ownField = GetField(Controller);
		return HasEmpty(ownField) && (HasUsed(ownField) || HasUsed(GetField(1 - Controller)));
	}
}