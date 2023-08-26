// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Weaponsmith : Creature
{
	public Weaponsmith() : base(
		Name: "Weaponsmith",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "Your other creatures gain +1/+1.",
		OriginalPower: 2,
		OriginalLife: 3
		)
	{ }

	public override void Init()
	{
		RegisterLingeringEffect(LingeringEffectInfo.Create(BuffEffect, this));
	}

	private void BuffEffect(Creature target)
	{
		foreach(Creature? card in GetField(target.Controller))
		{
			if(card != null && card != target)
			{
				card.Power += 1;
				card.Life += 1;
			}
		}
	}
}
