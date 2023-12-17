// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Inferno : Spell
{
	public Inferno() : base(
		Name: "Inferno",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 4,
		Text: "{Cast}: Cast \"Ignite\" on every creature on the field."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
	}

	private void CastEffect()
	{
		foreach(Creature card in GetBothFieldsUsed())
		{
			Cast(player: Controller, card: new Ignite(forcedTarget: card) { BaseController = this.Controller, Controller = this.Controller });
		}
	}
}
