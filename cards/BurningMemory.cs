// scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class BurningMemory : Spell
{
	public BurningMemory() : base(
		Name: "Burning Memory",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Create an Afterglow token with Power and Life of target creature in a grave and [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		Creature target = (Creature)SelectSingleCard(player: Controller, cards: GetForBoth(GetGrave), description: "Select creature to let glow");
		Token token = CreateToken(player: Controller, power: target.BasePower, life: target.BaseLife, name: "Afterglow");
		token.RegisterKeyword(Keyword.Decaying);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
	}

	public bool CastCondition()
	{
		return GetGrave(Controller).Length > 0 || GetGrave(1 - Controller).Length > 0;
	}

}
