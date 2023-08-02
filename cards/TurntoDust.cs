using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class TurntoDust : Spell
{
	public TurntoDust() : base(
		Name: "Turn to Dust",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 3,
		Text: "{Cast}: Target creature gains [Brittle]. {Revelation}: If a creature with [Brittle] died this turn: Draw 1."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: () => Draw(player: Controller, amount: 1), condition: () => GetBrittleDeathCountXTurnsAgo(player: Controller, turns: 0) > 0), referrer: this);
	}

	private void CastEffect()
	{
		SelectSingleCard(player: Controller, cards: GetForBoth(GetFieldUsed), description: "Select target to make Brittle").RegisterKeyword(Keyword.Brittle);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothWholeFields());
	}
}
