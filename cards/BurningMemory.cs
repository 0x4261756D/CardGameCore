// scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class BurningMemory : Spell
{
	public BurningMemory() : base(
		Name: "Burning Memory",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Cast}: Create an Afterglow token with Power and Life of target creature in a grave and [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetForBoth(GetGrave), amount: 1, description: "Select creature to let glow")[0];
		CreateToken(player: Controller, power: target.BasePower, life: target.BaseLife, name: "Afterglow").RegisterKeyword(Keyword.Decaying);
	}

	public bool CastCondition()
	{
		return GetGrave(Controller).Length > 0 || GetGrave(1 - Controller).Length > 0;
	}

}