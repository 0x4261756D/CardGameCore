// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class SugomaGodoftheAbyss : Creature
{
	public SugomaGodoftheAbyss() : base(
		Name: "Sugoma God of the Abyss",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 10,
		Text: "[Colossal] +5\nCannot be Discarded.\n{Activate}: Discard 1. Destroy target creature.\n{Revelation}: Take 1 damage then draw 1.",
		OriginalPower: 15,
		OriginalLife: 15
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Colossal, 5);
		RegisterActivatedEffect(info: new ActivatedEffectInfo(name: "Activate", effect: ActivatedEffect, condition: ActivatedCondition, referrer: this));
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect), referrer: this);
	}

	public void RevelationEffect()
	{
		PlayerChangeLife(player: Controller, amount: -1, source: this);
		Draw(player: Controller, amount: 1);
	}

	public void ActivatedEffect()
	{
		DiscardAmount(player: Controller, amount: 1);
		Creature target = SelectSingleCard(player: Controller, cards: GetBothFieldsUsed(), description: "Select card to destroy");
		Destroy(target);
	}

	public bool ActivatedCondition()
	{
		return HasUsed(GetBothFieldsWhole()) && GetDiscardable(Controller, ignore: null).Length > 0;
	}

	public override bool CanBeDiscarded() => false;
}
