// Scripted by 0x4261756D
using System;
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Equalize : Spell
{
	public Equalize() : base(
		Name: "Equalize",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 4,
		Text: "{Cast}: The player controlling the most creatures returns creatures to their hand until the players control the same amount of creatures.\n{Revelation}: Draw X cards where X is the difference between your and your opponent's amount of creatures."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect), referrer: this);
	}

	private void RevelationEffect()
	{
		Draw(player: Controller, amount: Math.Abs(GetFieldUsed(0).Length - GetFieldUsed(1).Length));
	}

	private void CastEffect()
	{
		int difference = GetFieldUsed(1).Length - GetFieldUsed(0).Length;
		if(difference == 0)
		{
			return;
		}
		int amount = Math.Abs(difference);
		int player = ((difference / amount) + 1) / 2;
		for(int i = 0; i < amount; i++)
		{
			MoveToHand(player: player, card: SelectSingleCard(player: player, cards: GetFieldUsed(player), description: "Select card to return to hand"));
		}
	}
}
