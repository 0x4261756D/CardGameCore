//Scripted by Dotlof
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class DragonsBreath : Spell
{
	public DragonsBreath() : base(
		Name: "Dragon's Breath",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 3,
		Text: "{Cast}: All creatures gain -2/-0 and [Decaying]."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	public void CastEffect()
	{
		foreach(Creature card in GetBothFieldsUsed())
		{
			card.RegisterKeyword(Keyword.Decaying);
			CreatureChangePower(target: card, amount: -2, source: this);
		}
	}

	private bool CastCondition()
	{
		return HasUsed(GetBothFieldsWhole());
	}

}
