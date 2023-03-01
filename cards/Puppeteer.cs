// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class Pupeteer : Creature
{
	public Pupeteer() : base(
		Name: "Pupeteer",
		CardClass: PlayerClass.All,
		OriginalCost: 2,
		Text: "{Activate}: Move target creature you control.",
		OriginalPower: 2,
		OriginalLife: 2
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}