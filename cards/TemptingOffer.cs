// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class TemptingOffer : Spell
{
	public TemptingOffer() : base(
		Name: "Tempting Offer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 0,
		Text: "{Cast}: Discard 2. Gain 2 Momentum."
		)
	{}
	// TODO: implement functionality

	public override void Init()
	{
	}
}