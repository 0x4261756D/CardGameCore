// Scripted by 0x4261756D
using CardGameCore;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class FlashingFire : Spell
{
	public FlashingFire() : base(
		Name: "Flashing Fire",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 1,
		Text: "{Cast}: Deal X damage to any target where X is the number of times you cast \"Flashing Fire\" this turn. If this destroys a creature, return this to your hand."
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
	}

	private void CastEffect()
	{
		Creature[] fields = GetBothFieldsUsed();
		int damage = -GetCastCount(player: Controller, name: Name);
		if(fields.Length > 0 && AskYesNo(player: Controller, question: "Damage creature?"))
		{
			Creature target = SelectSingleCard(player: Controller, cards: fields, description: "Select target to damage");
			CreatureChangeLife(target, amount: damage, source: this);
			if(!target.Location.HasFlag(Location.Field))
			{
				MoveToHand(player: Controller, card: this);
			}
		}
		else
		{
			PlayerChangeLife(player: AskYesNo(player: Controller, question: "Damage the opponent?") ? 1 - Controller : Controller, amount: damage, source: this);
		}
	}
}
