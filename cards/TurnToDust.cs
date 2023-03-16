using CardGameCore;
using static CardGameUtils.GameConstants;

class TurnToDust : Spell
{
	public TurnToDust() : base(
		Name: "Turn to Dust",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 4,
		Text: "{Cast}: Target creature gains [Brittle]. {Revelation}: If a creature with [Brittle] died this turn: Draw 1."
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}