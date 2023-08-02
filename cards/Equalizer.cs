//Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Equalizer : Spell
{
	public Equalizer() : base(
		Name: "Equalizer",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: Target creatures attack and life become the greater of the two values."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetForBoth(GetFieldUsed), description: "Target creature to equalize stats");
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: target));
	}

	public void BuffEffect(Card target)
	{
		bool useLife = (target.Life >= target.Power);
		if(useLife)
		{
			target.Power = target.Life;
		}
		else target.Life = target.Power;
	}

	public bool CastCondition()
	{
		return GetFieldUsed(Controller).Length > 0;
	}


}
