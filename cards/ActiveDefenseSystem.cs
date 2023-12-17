// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

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
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect, condition: CastCondition), referrer: this);
	}

	private void CastEffect()
	{
		Token token = CreateToken(player: Controller, power: 1, life: 1, name: "Automaton");
		token.Text = TokenText;
		RegisterDeathTrigger(trigger: new CreatureTargetingTrigger(effect: TokenDeathEffect, influenceLocation: Location.Field), referrer: token);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
	}

	private void TokenDeathEffect(Creature target)
	{
		Draw(player: target.Controller, amount: 1);
		if(ContainsValid(GetGrave(target.Controller), isValid: EndPhaseFilter))
		{
			MoveToHand(player: target.Controller, card: SelectSingleCard(player: target.Controller, cards: FilterValid(GetGrave(target.Controller), EndPhaseFilter), $"Target \"{Name}\" to return to hand"));
		}
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: EndPhaseEffect, condition: EndPhaseCondition, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: target);
	}

	private bool EndPhaseCondition()
	{
		return ContainsValid(GetHand(Controller), EndPhaseFilter);
	}

	private void EndPhaseEffect()
	{
		Discard(SelectSingleCard(Controller, FilterValid(GetHand(Controller), EndPhaseFilter), description: $"Select \"{Name}\" to discard"));
	}

	private bool EndPhaseFilter(Card card) => card.Name == this.Name;

	private bool CastCondition()
	{
		return HasEmpty(GetField(Controller));
	}
}
