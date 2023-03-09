// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class GearUp : Spell
{
	public GearUp() : base(
		Name: "Gear Up",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 0,
		Text: "{Cast}: Target creature you control gets +2/+2 and [Brittle].",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

	public override void Init(){
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect(){
		Card target = SelectCards(player: Controller, cards: GetBothFieldsUsed(), amount: 1, description: "Target creature to reinforce")[0];
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: target));
	}


	public void BuffEffect(Card target){
		target.Life += 2;
		target.Power += 2;
		target.RegisterKeyword(Keyword.Brittle);
	}

	public bool CastCondition(){
		return GetFieldUsed(Controller).Length > 0;
	}

}