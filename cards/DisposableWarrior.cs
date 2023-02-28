// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class DisposableWarrior : Creature
{
	public DisposableWarrior() : base(
		Name: "Disposable Warrior",
		CardClass: PlayerClass.All,
		OriginalCost: 0,
		Text: "{Revelation}: Cast this.",
		OriginalPower: 1,
		OriginalLife: 1
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}