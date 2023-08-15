// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

class ReplicatingGolem : Creature
{
	public ReplicatingGolem() : base(
		Name: "Replicating Golem",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 3,
		Text: "{Attack}: Create a token copy of this card with [Brittle].",
		OriginalPower: 3,
		OriginalLife: 2
		)
	{ }

	public override void Init()
	{
		RegisterAttackTrigger(trigger: new Trigger(effect: AttackEffect, condition: () => HasEmpty(GetField(Controller))), referrer: this);
	}

	public void AttackEffect()
	{
		Card token = CreateTokenCopy(player: Controller, card: this);
		token.RegisterKeyword(Keyword.Brittle);
		MoveToField(targetPlayer: Controller, choosingPlayer: Controller, card: token);
	}

}
