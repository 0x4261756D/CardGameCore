// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class Reinforce : Spell
{
	public Reinforce() : base(
		Name: "Reinforce",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: Target creature gains +0/+2 it loses [Brittle].\n{Revelation}: Gain 2 life."
		)
	{ }
	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: GainEffect), referrer: this);
	}

	public void CastEffect(){
		Card target = SelectCards(player: Controller, cards: GetBothFieldsUsed(), amount: 1, description: "Target creature to reinforce")[0];
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: target));
	}

	private void GainEffect()
	{
		PlayerChangeLife(player: Controller, amount: 2);
	}


	public void BuffEffect(Card target){
		target.Life += 2;
		target.Keywords.Remove(Keyword.Brittle);
	}

	public bool CastCondition(){
		return GetBothFieldsUsed().Length > 0;
	}

}