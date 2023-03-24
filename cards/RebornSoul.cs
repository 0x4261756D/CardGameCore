// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class RebornSoul : Creature
{
	public RebornSoul() : base(
		Name: "Reborn Soul",
		CardClass: PlayerClass.Cultist,
		Text: "{Discard}: Cast this.\n{Revelation}: You may discard 1, if you do, create a 1/1 Weaker Soul token.",
		OriginalCost: 4,
		OriginalLife: 2,
		OriginalPower: 3
		)
	{ }

	public override void Init()
	{
		RegisterDiscardTrigger(trigger: new DiscardTrigger(effect: () => Cast(player: Controller, card: this), condition: () => HasEmpty(GetField(Controller))), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect, condition: () => GetDiscardable(Controller).Length > 0 && HasEmpty(GetField(Controller))), referrer: this);
	}

	public void RevelationEffect()
	{
		if(AskYesNo(player: Controller, question: "Discard?"))
		{
			DiscardAmount(player: Controller, amount: 1);
			CreateToken(player: Controller, power: 1, life: 1, name: "Weaker Soul");
		}
	}
}