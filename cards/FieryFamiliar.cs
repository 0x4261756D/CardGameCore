// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class FieryFamiliar : Creature
{
	public FieryFamiliar() : base(
		Name: "Fiery Familiar",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "Your \"Ignite\" deals +1 damage \n{Revelation}: Add this to your hand.",
		OriginalPower: 1,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterLingeringEffect(info: new LingeringEffectInfo(effect: BuffEffect, referrer: this));
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	private void RevelationEffect()
	{
		MoveToHand(player: Controller, card: this);
	}

	private void BuffEffect(Card target)
	{
		ChangeIgniteDamageTemporary(player: Controller, amount: 1);
	}
}