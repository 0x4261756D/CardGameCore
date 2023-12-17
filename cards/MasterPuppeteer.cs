// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class MasterPuppeteer : Creature
{
	public MasterPuppeteer() : base(
		Name: "Master Puppeteer",
		CardClass: PlayerClass.All,
		OriginalCost: 3,
		Text: "{Activate}: Move target creature.",
		OriginalPower: 3,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterActivatedEffect(info: new ActivatedEffectInfo(name: "Activate", effect: MoveEffect, condition: MoveCondition, referrer: this));
	}

	private void MoveEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: FilterValid(GetBothFieldsUsed(), FilterMovable), "Select target to move");
		MoveToField(choosingPlayer: Controller, targetPlayer: target.Controller, card: target, source: this);
	}

	private bool FilterMovable(Creature card) => !card.Keywords.ContainsKey(Keyword.Immovable);

	private bool MoveCondition()
	{
		return ContainsValid(cards: GetBothFieldsUsed(), isValid: FilterMovable);
	}
}
