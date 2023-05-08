//Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class DragonsBreath : Spell
{
	public DragonsBreath() : base(
		Name: "Dragon's Breath",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Cast}: All creatures gain -2/-0 and [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		foreach(Card card in GetForBoth(GetFieldUsed))
		{
			card.RegisterKeyword(Keyword.Decaying);
			RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: DebuffEffect, referrer: card));
		}
	}

	public void DebuffEffect(Card target)
	{
		target.Power -= 2;
	}

	private bool CastCondition()
	{
		return GetForBoth(GetFieldUsed).Length > 0;
	}

}