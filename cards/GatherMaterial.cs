// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class GatherMaterial : Spell
{
	public GatherMaterial() : base(
		Name: "Gather Material",
		CardClass: PlayerClass.Artificer,
		OriginalCost: 1,
		Text: "{Cast}: [Gather] 3. If the gathered card is a creature with [Brittle] gain 1 Momentum.\n{Revelation}: If you control a creature with [Brittle], add this to your hand.",
		CanBeClassAbility: true
		)
	{ }
	// TODO: implement functionality

	public override void Init()
	{
	}

}