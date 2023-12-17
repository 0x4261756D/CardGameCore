// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

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
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect, condition: RevelationCondition), referrer: this);
	}

	private bool RevelationCondition()
	{
		return HasEmpty(GetField(Controller));
	}

	private void RevelationEffect()
	{
		Token token = CreateToken(player: Controller, power: 3, life: 2, name: "Drone");
		token.RegisterKeyword(Keyword.Brittle);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
	}

	private void CastEffect()
	{
		for(int i = 0; i < 2 && HasEmpty(GetField(Controller)); i++)
		{
			CreateTokenOnField(player: Controller, power: 3, life: 2, name: "Drone", source: this);
		}
	}

}
