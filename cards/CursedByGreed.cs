// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class CursedbyGreed : Spell
{
	public CursedbyGreed() : base(
		Name: "Cursed by Greed",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Pay 3 life. Draw 2.\n{Discard}: Draw 3."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterDiscardTrigger(trigger: new Trigger(effect: DiscardEffect), referrer: this);
	}

	public void DiscardEffect()
	{
		Draw(player: Controller, amount: 3);
	}

	public void CastEffect()
	{
		PayLife(player: Controller, amount: 3);
		Draw(player: Controller, amount: 2);
	}

}
