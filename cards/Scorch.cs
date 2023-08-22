//Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Scorch : Spell
{
	public Scorch() : base(
		Name: "Scorch",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Cast}: Target creature's attack becomes 0. It gains [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Target creature to scorch");
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: ScorchEffect, referrer: target));
	}

	public void ScorchEffect(Card t)
	{
		Creature target = (Creature)t;
		target.Power = 0;
		target.RegisterKeyword(Keyword.Decaying);
	}

	private bool CastCondition()
	{
		return GetBothFieldsUsed().Length > 0;
	}



}
