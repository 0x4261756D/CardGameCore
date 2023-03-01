using CardGameCore;
using static CardGameUtils.GameConstants;

class FlashingFire : Spell
{
	public FlashingFire() : base(
		Name: "Flashing Fire",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Deal X damage to any target where X is the number of times you cast \"Flashing Fire\" this turn \n {Kill:} Return this to your hand."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}