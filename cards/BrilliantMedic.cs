// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class BrilliantMedic : Creature
{
	public BrilliantMedic() : base(
		Name: "Brilliant Medic",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Cast}: Heal target creature or player by 4.",
		OriginalPower: 2,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: HealEffect, condition: () => true), referrer: this);
	}

	public void HealEffect()
	{
		if(!HasUsed(GetForBoth(GetFieldUsed)) || AskYesNo(player: Controller, question: "Heal player?"))
		{
			PlayerChangeLife(player: AskYesNo(player: Controller, question: "Heal you?") ? Controller : 1 - Controller, amount: 4);
		}
		else
		{
			Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select healing target")[0];
			RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: HealCardEffect, referrer: target));
		}
	}

	public void HealCardEffect(Card target)
	{
		target.Life += 4;
	}
}