// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Unbury : Spell
{
	public Unbury() : base(
		Name: "Unbury",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
		Text: "{Cast}: Return all creatures from the graves to the decks."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
	}

	public void CastEffect()
	{
		ReturnCardsToDeck(FilterValid(cards: GetForBoth(GetGrave), card => card.CardType == CardType.Creature));
	}

}