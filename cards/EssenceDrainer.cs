// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EssenceDrainer : Creature
{
	public EssenceDrainer() : base(
		Name: "Essence Drainer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Gain +2/+2 for each card you discarded this turn.",
		OriginalPower: 1,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect(){
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: this));
	}

	public void BuffEffect(Card target){
		target.Life += GetDiscardCountThisTurn(player: Controller);
		target.Power += GetDiscardCountThisTurn(player: Controller);
	}

	public bool CastCondition(){
		return true;
	}

}