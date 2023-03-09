// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EternalArmy : Spell
{
	public EternalArmy() : base(
		Name: "Eternal Army",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 6,
		Text: "{Cast}: Create any number of 5/1 Construct tokens with \"{Death}: Create a 1/1 Construct.\"."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect(){
		for(int emptyZones = FIELD_SIZE - GetFieldUsed(Controller).Length; emptyZones >= 0; emptyZones--){
			CreateToken(player: Controller, power: 5, life: 1, name: "Construct");
		}
		//Implement giving Death Trigger
	}

	public bool CastCondition(){
		return true;
	}

}