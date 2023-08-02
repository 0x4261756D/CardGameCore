// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class GearUp : Spell
{
	public GearUp() : base(
		Name: "Gear Up",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 0,
		Text: "{Cast}: Target creature you control gets +2/+2."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetFieldUsed(Controller), description: "Target creature to reinforce");
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: target));
	}


	public void BuffEffect(Card target)
	{
		target.Life += 2;
		target.Power += 2;
	}

	public bool CastCondition()
	{
		return GetFieldUsed(Controller).Length > 0;
	}

}
