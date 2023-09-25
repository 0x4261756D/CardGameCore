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
		Text: "[Brittle]\n{Victorious}: Gain +2/+2 and lose [Brittle] this turn.\n{Revelation}: Target creature with less than 7 power gains [Immovable] until the end of turn.",
		OriginalPower: 6,
		OriginalLife: 4
		)
	{ }

	public override void Init()
	{
		RegisterKeyword(Keyword.Brittle);
		RegisterVictoriousTrigger(trigger: new Trigger(effect: VictoriousEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect, condition: RevelationCondition), referrer: this);
	}

	public void VictoriousEffect()
	{
		RegisterTemporaryLingeringEffect(info: LingeringEffectInfo.Create(effect: BoostEffect, referrer: this));
		if(Keywords.Remove(Keyword.Brittle))
		{
			RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: ResetBrittleEffect, state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: this);
		}
	}

	private void BoostEffect(Creature target)
	{
		target.Life += 2;
		target.Power += 2;
	}

	public void ResetBrittleEffect()
	{
		RegisterKeyword(Keyword.Brittle);
	}

	public void RevelationEffect()
	{
		Creature target = SelectSingleCard(player: Controller, cards: FilterValid(cards: GetBothFieldsUsed(), isValid: Filter), description: "Select creature to frighten");
		target.RegisterKeyword(Keyword.Immovable);
		RegisterStateReachedTrigger(trigger: new StateReachedTrigger(effect: () => target.Keywords.Remove(Keyword.Immovable), state: State.TurnEnd, influenceLocation: Location.ALL, oneshot: true), referrer: target);
	}

	public bool RevelationCondition()
	{
		return ContainsValid(cards: GetBothFieldsUsed(), isValid: Filter);
	}

	public bool Filter(Creature card) => card.Power < 7;

}
