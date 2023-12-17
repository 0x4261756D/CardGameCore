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
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterDiscardTrigger(trigger: new Trigger(effect: DiscardEffect, condition: DiscardCondition), referrer: this);
	}

	public void DiscardEffect()
	{
		SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Select card to inflict the Curse of Sugoma upon").RegisterKeyword(Keyword.Decaying);
	}
	public bool DiscardCondition()
	{
		return HasUsed(GetBothFieldsWhole());
	}

	public void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Target creature to curse");
		RegisterTemporaryLingeringEffect(info: LingeringEffectInfo.Create(effect: CurseEffect, referrer: target));
	}

	public void CurseEffect(Creature target)
	{
		target.Power += 2;
		target.Life -= 1;
	}

	private bool CastCondition()
	{
		return HasUsed(GetField(Controller));
	}

}
