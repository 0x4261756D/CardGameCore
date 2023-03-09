// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class FieryIllusion : Spell
{
	public FieryIllusion() : base(
		Name: "Fiery Illusion",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Create two 0/1 Illusions with [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect(){
		CreateToken(player: Controller, power: 0, life: 1, name: "Illusion").RegisterKeyword(Keyword.Decaying);
		CreateToken(player: Controller, power: 0, life: 1, name: "Illusion").RegisterKeyword(Keyword.Decaying);
	}

	public bool CastCondition(){
		return FIELD_SIZE - GetFieldUsed(Controller).Length > 1;
	}
}