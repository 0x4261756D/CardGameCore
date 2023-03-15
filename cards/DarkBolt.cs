// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class DarkBolt : Spell
{
	public DarkBolt() : base(
		Name: "Dark Bolt",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Deal 3 damage any target.\n{Discard}: Cast this."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
		RegisterDiscardTrigger(trigger: new DiscardTrigger(effect: DiscardEffect), referrer: this);
	}

	private void DiscardEffect()
	{
		Cast(player: Controller, card: this);
	}

	private void CastEffect()
	{
		if(HasUsed(GetBothWholeFields()) && AskYesNo(player: Controller, question: "Target a creature?"))
		{
			Card target = SelectCards(player: Controller, cards: GetForBoth(GetFieldUsed), amount: 1, description: "Select target")[0];
			RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: DamageEffect, referrer: target));
		}
		else
		{
			PlayerChangeLife(player: AskYesNo(player: Controller, question: "Damage opponent?") ? 1 - Controller : Controller, amount: 3);
		}	
	}

	private void DamageEffect(Card target)
	{
		target.Life -= 3;
	}
}