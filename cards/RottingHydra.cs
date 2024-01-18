// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class RottingHydra : Creature
{
	public RottingHydra() : base(
		Name: "Rotting Hydra",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 5,
		Text: "[Decaying]\n{Victorious}: Gain +0/+3.",
		OriginalPower: 5,
		OriginalLife: 6
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Decaying);
		RegisterVictoriousTrigger(trigger: new Trigger(effect: VictoriousEffect), referrer: this);
	}

	public void VictoriousEffect()
	{
		CreatureChangeLife(target: this, amount: 3, source: this);
	}
}
