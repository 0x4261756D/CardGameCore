// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class DarkBolt : Spell
{
	public DarkBolt() : base(
		Name: "Dark Bolt",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 1,
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
		ChangeLifeOfAnyTarget(player: Controller, amount: -3, description: "Bolt");
	}
}