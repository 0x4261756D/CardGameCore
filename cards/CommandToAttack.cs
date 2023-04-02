//Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class CommandtoAttack : Spell
{
	public CommandtoAttack() : base(
		Name: "Command to Attack",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 3,
		Text: "{Cast}: For each of your opponents creatures create a 1/1 Construct token with [Brittle] and [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card[] enemyCards = GetFieldUsed(1 - Controller);
		foreach(Card card in enemyCards)
		{
			Card token = CreateToken(player: Controller, power: 1, life: 1, name: "Construct");
			token.RegisterKeyword(Keyword.Decaying);
			token.RegisterKeyword(Keyword.Brittle);
		}
	}

	private bool CastCondition()
	{
		return GetFieldUsed(1 - Controller).Length <= GetFieldUsed(Controller).Length;
	}

}