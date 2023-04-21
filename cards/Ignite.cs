// Scripted by 0x4261756D
using CardGameCore;
using CardGameUtils.Structs;
using static CardGameCore.CardUtils;
using static CardGameUtils.GameConstants;

class Ignite : Spell
{
	public Ignite() : base(
		Name: "Ignite",
		CardClass: PlayerClass.Pyromancer,
		OriginalCost: 0,
		Text: "{Cast}: Deal 1 damage to any target.\n{Revelation}: Cast this.",
		CanBeClassAbility: true
		)
	{ }

	private Card? forcedTarget = null;

	public Ignite(Card forcedTarget) : this()
	{
		this.forcedTarget = forcedTarget;
	}

	public override void Init()
	{
		RegisterCastTrigger(trigger: new CastTrigger(effect: IgniteEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new RevelationTrigger(effect: RevelationEffect), referrer: this);
	}

	public void IgniteEffect()
	{
		if(forcedTarget == null)
		{
			ChangeLifeOfAnyTarget(player: Controller, amount: -GetIgniteDamage(Controller), description: "Damage", source: this);
		}
		else
		{
			int damage = GetIgniteDamage(Controller);
			RegisterTemporaryLingeringEffect(info: new LingeringEffectInfo(effect: (target) => { target.Life -= damage; }, referrer: forcedTarget));
		}
	}

	public void RevelationEffect()
	{
		Cast(card: this, player: Controller);
	}

	public override CardStruct ToStruct(bool client = false)
	{
		if(!client)
		{
			this.Text = $"{{Cast}}: Deal {GetIgniteDamage(Controller)} damage to any target.\n{{Revelation}}: Cast this.";
		}
		return base.ToStruct(client);
	}

}