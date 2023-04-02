// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class RebornSoul : Creature
{
	public RebornSoul() : base(
		Name: "Reborn Soul",
		CardClass: PlayerClass.Cultist,
		Text: "{Discard}: Cast this.\n{Revelation}: You may discard 1, if you do, create a 2/2 Weaker Soul token.",
		OriginalCost: 4,
		OriginalLife: 3,
		OriginalPower: 5
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
			CreateToken(player: Controller, power: 2, life: 2, name: "Weaker Soul");
		}
	}
}