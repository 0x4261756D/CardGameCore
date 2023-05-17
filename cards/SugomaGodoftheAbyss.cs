// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class SugomaGodoftheAbyss : Creature
{
	public SugomaGodoftheAbyss() : base(
		Name: "Sugoma God of the Abyss",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 12,
		Text: "[Collossal] +5\nCannot be Discarded.\n{Activate}: Discard 1. Destroy target creature.\n{Revelation}: Take 1 damage then draw 1.",
		OriginalPower: 15,
		OriginalLife: 15
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Colossal, 5);
		RegisterActivatedEffect(info: new ActivatedEffectInfo(name: "Activate", effect: ActivatedEffect, condition: ActivatedCondition, referrer: this));
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	public void RevelationEffect()
	{
		PlayerChangeLife(player: Controller, amount: -1, source: this);
		Draw(player: Controller, amount: 1);
	}

	public void ActivatedEffect()
	{
		DiscardAmount(player: Controller, amount: 1);
		Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select card to destroy")[0];
		Destroy(target);
	}

	public bool ActivatedCondition()
	{
		return HasUsed(GetBothWholeFields()) && GetDiscardable(Controller, ignore: null).Length > 0;
	}

	public override bool CanBeDiscarded()
	{
		return false;
	}

}