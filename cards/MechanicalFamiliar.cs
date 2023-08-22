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
		Text: "{You create a token with [Brittle] except by \"Mechanical Familiar\"}: You may create another one.\n{Revelation}: Add this to your hand.",
		OriginalPower: 1,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterTokenCreationTrigger(trigger: new TokenCreationTrigger(effect: CreationEffect, condition: CreationCondition), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	private bool CreationCondition(Creature token, Card source)
	{
		return source.Name != this.Name && token.Controller == Controller && token.Keywords.ContainsKey(Keyword.Token) && token.Keywords.ContainsKey(Keyword.Brittle) &&
			HasEmpty(GetField(Controller));
	}

	private void CreationEffect(Creature token, Card source)
	{
		if(AskYesNo(player: Controller, question: "Create another one?"))
		{
			CreateTokenCopyOnField(player: Controller, card: token, source: this);
		}
	}

	private void RevelationEffect()
	{
		MoveToHand(player: Controller, card: this);
	}
}
