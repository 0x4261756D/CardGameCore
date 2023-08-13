// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class BrittleBehemoth : Creature
{
	public BrittleBehemoth() : base(
		Name: "Brittle Behemoth",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 4,
		Text: "[Brittle]\n{Victorious}: Gain +2/+2 and lose [Brittle] this turn.\n{Revelation}: Target creature with less than 7 power can't move this turn.",
		OriginalPower: 6,
		OriginalLife: 4
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Brittle);
		RegisterVictoriousTrigger(trigger: new Trigger(effect: VictoriousEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect, condition: RevelationCondition), referrer: this);
	}

	public void VictoriousEffect()
	{
		CreatureChangeLife(target: this, amount: 2, source: this);
		RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: (target) => target.Power += 2, referrer: this));
		if(Keywords.Remove(Keyword.Brittle))
		{
			RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: ResetBrittleEffect, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: this);
		}
	}

	public void ResetBrittleEffect()
	{
		RegisterKeyword(Keyword.Brittle);
	}

	public void RevelationEffect()
	{
		Card target = SelectSingleCard(player: Controller, cards: FilterValid(cards: GetForBoth(GetFieldUsed), isValid: Filter), description: "Select creature to frighten");
		target.CanMove = false;
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: () => target.CanMove = true, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: target);
	}

	public bool RevelationCondition()
	{
		return ContainsValid(cards: GetForBoth(GetFieldUsed), isValid: Filter);
	}

	public bool Filter(Card card) => card.Power < 7;

}
