// scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Shatter : Spell
{
	public Shatter() : base(
		Name: "Shatter",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: Destroy target creature you control. Deal damage to your opponent equal to its power."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: () => HasUsed(GetField(Controller))), referrer: this);
	}

	public void CastEffect()
	{
		Card target = SelectCards(player: Controller, cards: GetFieldUsed(Controller), amount: 1, description: "Select target to destroy")[0];
		int damage = target.Power;
		Destroy(target);
		PlayerChangeLife(player: 1 - Controller, amount: -damage, source: this);
	}

}