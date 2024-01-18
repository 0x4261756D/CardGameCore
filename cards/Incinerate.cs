// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Incinerate : Spell
{
	public Incinerate() : base(
		Name: "Incinerate",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 6,
		Text: "{Cast}: Destroy all creatures. Deal X damage to your opponent where X is the power of all those creatures."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: DestroyEffect), referrer: this);
	}

	private void DestroyEffect()
	{
		int damage = 0;
		foreach(Creature card in GetBothFieldsUsed())
		{
			damage -= card.Power;
			Destroy(card);
		}
		PlayerChangeLife(player: 1 - Controller, amount: damage, this);
	}
}
