// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Weaponsmith : Creature
{
	public Weaponsmith() : base(
		Name: "Weaponsmith",
		CardClass: PlayerClass.All,
		OriginalCost: 4,
		Text: "Other creatures you control gain +1/+1.",
		OriginalPower: 4,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterLingeringEffect(effect: BuffEffect, this);
	}

	private void BuffEffect()
	{
		foreach (Card? card in GetField(this.Controller))
		{
			if(card != null)
			{
				card.Power += 1;
				card.Life += 1;
			}
		}
	}
}