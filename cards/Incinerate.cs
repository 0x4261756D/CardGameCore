// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class Incinerate : Spell
{
	public Incinerate() : base(
		Name: "Incinerate",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 6,
		Text: "{Cast}: Destroy all creatures."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: DestroyEffect), referrer: this);
	}

	private void DestroyEffect()
	{
		foreach(Card card in GetForBoth(GetFieldUsed))
		{
			Destroy(card);
		}
	}
}