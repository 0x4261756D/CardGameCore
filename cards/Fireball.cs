// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Fireball : Spell
{
	public Fireball() : base(
		Name: "Fireball",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 2,
		Text: "{Cast}: Deal 4 damage to any target.\n{Revelation}: If your opponent controls a damaged creature, add this to your hand."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}
}