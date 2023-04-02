// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Inferno : Spell
{
	public Inferno() : base(
		Name: "Inferno",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Cast}: Cast \"Ignite\" on every creature on the field."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
	}

	private void CastEffect()
	{
		foreach(Card card in GetForBoth(GetFieldUsed))
		{
			Cast(player: Controller, card: new Ignite(forcedTarget: card));
		}
	}
}