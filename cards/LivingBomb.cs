using CardGameCore;
using static CardGameUtils.GameConstants;

class LivingBomb : Spell
{
	public LivingBomb() : base(
		Name: "Living Bomb",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 2,
		Text: "{Cast}: Target creature gains \"{Death:} Deal X damage to this cards owner, where X is this cards attack\"."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}