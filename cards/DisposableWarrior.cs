// Scripted by Dotlof
using CardGameCore;
using static CardGameCore.CardUtils;
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
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect, condition: CastCondition), referrer: this);
	}

	public void RevelationEffect()
	{
		Cast(Controller, this);
	}

	private bool CastCondition()
	{
		return HasEmpty(GetField(Controller));
	}

}
