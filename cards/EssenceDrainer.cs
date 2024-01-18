// Scripted by 0x4261756D
using CardGameCore;
using static CardGameUtils.GameConstants;

class EssenceDrainer : Creature
{
	public EssenceDrainer() : base(
		Name: "Essence Drainer",
		CardClass: PlayerClass.Cultist,
		OriginalCost: 2,
		Text: "{Cast}: Gain +2/+2 for each card you discarded this turn.",
		OriginalPower: 1,
		OriginalLife: 1
		)
	{ }

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: CastEffect), referrer: this);
	}

	public void CastEffect()
	{
		int count = GetDiscardCountXTurnsAgo(player: Controller, turns: 0);
		RegisterTemporaryLingeringEffect(info: LingeringEffectInfo.Create(effect: (target) =>
		{
			target.Power += count * 2;
			target.Life += count * 2;
		}, referrer: this));
	}
}
