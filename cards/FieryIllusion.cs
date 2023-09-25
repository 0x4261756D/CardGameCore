// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class FieryIllusion : Spell
{
	public FieryIllusion() : base(
		Name: "Fiery Illusion",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 0,
		Text: "{Cast}: Create two 0/1 Illusions with [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		for(int i = 0; i < 2; i++)
		{
			Token token = CreateToken(player: Controller, power: 0, life: 1, name: "Illusion");
			token.RegisterKeyword(Keyword.Decaying);
			MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
		}
	}

	public bool CastCondition()
	{
		return FIELD_SIZE - GetFieldUsed(Controller).Length > 1;
	}
}
