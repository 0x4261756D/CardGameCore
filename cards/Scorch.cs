//Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class Scorch : Spell
{
	public Scorch() : base(
		Name: "Scorch",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Cast}: Target creatures attacks becomes 0. It gains [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetBothFieldsUsed(), amount: 1, description: "Target creature to scorch")[0];
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: ScorchEffect, referrer: target));
	}

	public void ScorchEffect(Card target)
	{
		target.Power = 0;
		target.RegisterKeyword(Keyword.Decaying);
	}

	private bool CastCondition()
	{
		return GetBothFieldsUsed().Length > 0;
	}



}