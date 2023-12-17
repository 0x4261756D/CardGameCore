// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class GatherMaterial : Spell
{
	public GatherMaterial() : base(
		Name: "Gather Material",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: [Gather] 6. If the gathered card is a creature gain 1 Momentum.\n{Revelation}: If you control a creature with [Brittle], add this to your hand."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect, condition: RevelationCondition), referrer: this);
	}

	private bool RevelationCondition()
	{
		return ContainsValid(GetFieldUsed(Controller), Filter);
	}

	private bool Filter(Creature card)
	{
		return card.Keywords.ContainsKey(Keyword.Brittle);
	}

	private void RevelationEffect()
	{
		MoveToHand(player: Controller, card: this);
	}

	private void CastEffect()
	{
		Card target = Gather(player: Controller, amount: 6);
		if(target.CardType == CardType.Creature)
		{
			PlayerChangeMomentum(player: Controller, amount: 1);
		}
	}
}
