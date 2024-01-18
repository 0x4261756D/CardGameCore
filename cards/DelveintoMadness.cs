// Scripted by Dotlof
using CardGameCore;
using static CardGameUtils.GameConstants;

class DelveintoMadness : Spell
{
	public DelveintoMadness() : base(
		Name: "Delve into Madness",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Draw 1. Discard 1.\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect), referrer: this);
	}

	public void RevelationEffect()
	{
		Cast(Controller, this);
	}

	public void CastEffect()
	{
		Draw(Controller, 1);
		DiscardAmount(player: Controller, amount: 1);
	}

}
