// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class MechanicalFamiliar : Creature
{
	public MechanicalFamiliar() : base(
		Name: "Mechanical Familiar",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{You create a token with [Brittle]}: You may create another one.\n{Revelation}: Add this to your hand.",
		OriginalPower: 1,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterTokenCreationTrigger(trigger: new GenericCastTrigger(effect: CreateEffect, condition: CreateCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	private void RevelationEffect()
	{
		MoveToHand(player: Controller, card: this);
	}

	private bool CreateCondition(Card target)
	{
		return target.Controller == Controller && target.Keywords.ContainsKey(Keyword.Token) && target.Keywords.ContainsKey(Keyword.Brittle) &&
			HasEmpty(GetField(Controller));
	}

	private void CreateEffect(Card target)
	{
		if(AskYesNo(player: Controller, question: "Create another one?"))
		{
			CreateTokenCopyOnField(player: Controller, card: target);
		}
	}
}
