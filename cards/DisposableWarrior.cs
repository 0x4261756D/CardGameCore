// Scripted by Dotlof
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

	public override void Init()
	{
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect, CastCondition), referrer: this);
	}

	public void RevelationEffect()
	{
		Cast(Controller, this);
	}

	private bool CastCondition()
	{
		return GetFieldUsed(Controller).Length < FIELD_SIZE;
	}

}