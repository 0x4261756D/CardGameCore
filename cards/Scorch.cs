//Scripted by Dotlof
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

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
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Target creature to scorch");
		RegisterTemporaryLingeringEffect(info: LingeringEffectInfo.Create(effect: ScorchEffect, referrer: target));
	}

	public void ScorchEffect(Creature target)
	{
		target.Power = 0;
		target.RegisterKeyword(Keyword.Decaying);
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothFieldsWhole());
	}
}
