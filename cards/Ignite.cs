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

	private readonly Creature? forcedTarget;

	public Ignite(Creature forcedTarget) : this()
	{
		this.forcedTarget = forcedTarget;
	}

	public override void Init()
	{
		RegisterCastTrigger(trigger: new Trigger(effect: IgniteEffect), referrer: this);
		RegisterRevelationTrigger(trigger: new Trigger(effect: RevelationEffect), referrer: this);
	}

	public void IgniteEffect()
	{
		int damage = -GetIgniteDamage(Controller);
		if(forcedTarget == null)
		{
			ChangeLifeOfAnyTarget(player: Controller, amount: damage, description: "Damage", source: this);
		}
		else
		{
			CreatureChangeLife(target: forcedTarget, amount: damage, source: this);
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
			Text = $"{{Cast}}: Deal {GetIgniteDamage(Controller)} damage to any target.\n{{Revelation}}: Cast this.";
		}
		return base.ToStruct(client);
	}

}
