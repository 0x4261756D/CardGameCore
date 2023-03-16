// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;
using static CardGameCore.CardUtils;

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
		RegisterCastTrigger(trigger: new CastTrigger(effect: CastEffect), referrer: this);
	}

	private void CastEffect()
	{
		Card[] fields = GetForBoth(GetFieldUsed);
		int damage = GetCastCount(player: Controller, name: this.Name);
		bool killed = false;
		if(fields.Length > 0 && AskYesNo(player: Controller, question: "Damage"))
		{
			Card target = Card.SelectCards(player: Controller, cards: fields, amount: 1, description: "Select target to damage")[0];
			killed = target.Life <= damage;
			Card.RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: (_) => target.Life -= damage, referrer: target));
		}
		else
		{
			Card.PlayerChangeLife(player: Card.AskYesNo(player: Controller, question: "Damage the opponent?") ? 1 - Controller : Controller, amount: -damage);
		}
		if(killed)
		{
			AddToHand(player: Controller, card: this);
		}
	}
}