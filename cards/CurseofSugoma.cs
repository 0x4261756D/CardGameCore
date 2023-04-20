// Scripted by Dotlof and 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class CurseofSugoma : Spell
{
	public CurseofSugoma() : base(
		Name: "Curse of Sugoma",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Target creature gains +2/-1.\n{Discard}: Target creature gains decaying."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterDiscardTrigger(trigger: new DiscardTrigger(effect: DiscardEffect, condition: DiscardCondition), referrer: this);
	}

	public void DiscardEffect()
	{
		SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select card to inflict the Curse of Sugoma upon")[0].RegisterKeyword(Keyword.Decaying);
	}
	public bool DiscardCondition()
	{
		return HasUsed(GetBothWholeFields());
	}

	public void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Target creature to curse")[0];
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: CurseEffect, referrer: target));
	}

	public void CurseEffect(Card target)
	{
		target.Power += 2;
		target.Life -= 1;
	}

	private bool CastCondition()
	{
		return GetFieldUsed(Controller).Length > 0;
	}

}