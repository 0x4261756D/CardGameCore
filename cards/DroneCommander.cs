// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class DroneCommander : Creature
{
	public DroneCommander() : base(
		Name: "Drone Commander",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 3,
		Text: "{Cast}: Create two 3/2 Drone tokens.\n{Revelation}: Create a 3/2 Drone token with [Brittle].",
		OriginalPower: 3,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect, condition: RevelationCondition), referrer: this);
	}

	private bool RevelationCondition()
	{
		return HasEmpty(GetField(Controller));
	}

	private void RevelationEffect()
	{
		CreateToken(player: Controller, power: 3, life: 2, name: "Drone").RegisterKeyword(Keyword.Brittle);
	}

	// TODO: Should these tokens also have [Brittle]?
	private void CastEffect()
	{
		for(int i = 0; i < 2 && HasEmpty(GetField(Controller)); i++)
		{
			CreateToken(player: Controller, power: 3, life: 2, name: "Drone");
		}
	}

}