// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Tinker : Spell
{
	public Tinker() : base(
		Name: "Tinker",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: Summon two 2/1 Construct tokens with [Brittle]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private bool CastCondition()
	{
		return FIELD_SIZE - GetFieldUsed(Controller).Length > 1;
	}

	private void CastEffect()
	{
		for(int i = 0; i < 2; i++)
		{
			CreateToken(player: Controller, power: 2, life: 1, name: "Construct").RegisterKeyword(Keyword.Brittle);
		}
	}
}