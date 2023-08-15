// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class ActiveDefenseSystem : Spell
{
	public ActiveDefenseSystem() : base(
		Name: "Active Defense System",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: $"{{Cast}}: Create a 1/1 Automaton token with \"{TokenText}\"."
		)
	{ }
	private const string TokenText = "{Death}: Draw 1. Return 1 \"Active Defense System\" from your Grave to your hand.\n{End of turn}: If this card died this turn, discard 1\"Active Defense System\".";

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Card token = CreateToken(player: Controller, power: 1, life: 1, name: "Automaton");
		token.Text = TokenText;
		RegisterDeathTrigger(trigger: new TargetingTrigger(effect: TokenDeathEffect), referrer: token);
	}

	private void TokenDeathEffect(Card target)
	{
		Draw(target.Controller, 1);
		if(ContainsValid(GetGrave(target.Controller), isValid: (card) => card.Name == this.Name))
		{
			MoveToHand(player: target.Controller, card: SelectSingleCard(player: target.Controller, cards: GetGrave(target.Controller), $"Target \"{this.Name}\" to return to hand"));
		}
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: EndPhaseEffect, condition: EndPhaseCondition, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: target);
	}

	private bool EndPhaseCondition()
	{
		return ContainsValid(GetHand(Controller), EndPhaseFilter);
	}

	private void EndPhaseEffect()
	{
		SelectSingleCard(Controller, FilterValid(GetHand(Controller), EndPhaseFilter), description: $"Select \"{this.Name}\" to discard");
	}

	private bool EndPhaseFilter(Card card) => card.Name == this.Name;

	private bool CastCondition()
	{
		return HasEmpty(GetField((Controller)));
	}
}