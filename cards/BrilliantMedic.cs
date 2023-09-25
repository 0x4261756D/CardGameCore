// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class BrilliantMedic : Creature
{
	public BrilliantMedic() : base(
		Name: "Brilliant Medic",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Cast}: Heal target any target by 4.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: HealEffect, condition: () => true), referrer: this);
	}

	public void HealEffect()
	{
		Creature[] fields = FilterValid(cards: GetBothFieldsUsed(), isValid: (card) => card.Life < card.BaseLife);
		if(fields.Length == 0 || Card.AskYesNo(player: Controller, question: "Heal a player?"))
		{
			PlayerChangeLife(player: Card.AskYesNo(player: Controller, question: "Heal yourself?") ? Controller : 1 - Controller, amount: 4, source: this);
		}
		else
		{
			Creature target = SelectSingleCard(player: Controller, cards: fields, description: "Select target to heal");
			CreatureChangeLife(target: target, Math.Min(4, target.BaseLife - target.Life), source: this);
		}
	}
}
