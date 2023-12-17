// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Assemble : Spell
{
	public Assemble() : base(
		Name: "Assemble",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 0,
		Text: "{Cast}: Create a 2/1 Construct token with [Brittle].\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect, CastCondition), referrer: this);
	}

	public void RevelationEffect()
	{
		Cast(Controller, this);
	}

	public void CastEffect()
	{
		Token token = CreateToken(player: Controller, power: 2, life: 1, name: "Construct");
		token.RegisterKeyword(Keyword.Brittle);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
	}

	public bool CastCondition()
	{
		return HasEmpty(GetField(Controller));
	}
}
