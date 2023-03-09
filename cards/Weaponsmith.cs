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
		RegisterLingeringEffect(new LingeringEffectInfo(BuffEffect, this));
	}

	private void BuffEffect(Card _)
	{
		foreach(Card? card in GetField(this.Controller))
		{
			if(card != null && card != this)
			{
				card.Power += 1;
				card.Life += 1;
			}
		}
	}
}