// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class CurseofSugoma : Spell
{
	public CurseofSugoma() : base(
		Name: "Curse of Sugoma",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Target creature gains +2/-1.\n{Discard}: Target creature gains decaying."
		)
	{ }
	// TODO: implement Discard effect

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
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