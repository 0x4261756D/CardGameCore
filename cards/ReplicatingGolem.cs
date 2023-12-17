// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class ReplicatingGolem : Creature
{
	public ReplicatingGolem() : base(
		Name: "Replicating Golem",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 3,
		Text: "{Victorious or deals damage}: Create a token copy of this card with [Brittle].",
		OriginalPower: 3,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterVictoriousTrigger(trigger: new Trigger(effect: DuplicateEffect, condition: () => HasEmpty(GetField(Controller))), referrer: this);
		RegisterDealsDamageTrigger(trigger: new Trigger(effect: DuplicateEffect, condition: () => HasEmpty(GetField(Controller))), referrer: this);
	}

	public void DuplicateEffect()
	{
		Creature token = CreateTokenCopy(player: Controller, card: this);
		token.RegisterKeyword(Keyword.Brittle);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token, source: this);
	}

}
