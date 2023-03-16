// Scripted by Dotlof and 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class EternalArmy : Spell
{
	public EternalArmy() : base(
		Name: "Eternal Army",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 6,
		Text: "{Cast}: Create any number of 5/1 Construct tokens with \"{Death}: Create a 1/1 Construct.\"."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
	}

	public void CastEffect()
	{
		for(int emptyZones = FIELD_SIZE - GetFieldUsed(Controller).Length; emptyZones >= 0; emptyZones--)
		{
			if(AskYesNo(player: Controller, question: "Create another Construct?"))
			{
				Card token = CreateToken(player: Controller, power: 5, life: 1, name: "Construct");
				RegisterDeathTrigger(trigger: new Trigger(effect: DeathEffect, condition: DeathCondition), referrer: token);
			}
			else
			{
				break;
			}
		}
	}

	private bool DeathCondition()
	{
		return HasEmpty(GetField(Controller));
	}

	private void DeathEffect()
	{
		CreateToken(player: Controller, power: 1, life: 1, name: "Construct");
	}
}