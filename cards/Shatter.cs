// scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Shatter : Spell
{
	public Shatter() : base(
		Name: "Shatter",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 0,
		Text: "{Cast}: Destroy target creature you control. Deal damage to your opponent equal to its power."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: () => HasUsed(GetField(Controller))), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: GetFieldUsed(Controller), description: "Select target to destroy");
		int damage = target.Power;
		Destroy(target);
		PlayerChangeLife(player: 1 - Controller, amount: -damage, source: this);
	}

}
