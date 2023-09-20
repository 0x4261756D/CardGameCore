using CardGameUtils;

namespace CardGameCore;

public enum Keyword
{
	Colossal,
	Brittle,
	Token,
	Decaying,
	Immovable,
}

public class Trigger
{
	public TriggerCondition condition;
	public Effect effect;

	public Trigger(Effect effect, TriggerCondition condition)
	{
		this.effect = effect;
		this.condition = condition;
	}

	public Trigger(Effect effect)
	{
		this.effect = effect;
		this.condition = () => true;
	}

	// NOTE: This is only used for inheritance
	protected Trigger()
	{
		effect = () => { };
		condition = () => true;
	}
}

public class TargetingTrigger
{
	public TargetingCondition condition;
	public TargetingEffect effect;

	public TargetingTrigger(TargetingEffect effect, TargetingCondition condition)
	{
		this.effect = effect;
		this.condition = condition;
	}

	public TargetingTrigger(TargetingEffect effect)
	{
		this.effect = effect;
		this.condition = (_) => true;
	}
}

public class DiscardTrigger : Trigger
{
	public GameConstants.Location influenceLocation;

	public DiscardTrigger(Effect effect, TriggerCondition condition, GameConstants.Location influenceLocation = GameConstants.Location.Field)
		: base(effect, condition)
	{
		this.influenceLocation = influenceLocation;
	}
	public DiscardTrigger(Effect effect, GameConstants.Location influenceLocation = GameConstants.Location.Field)
		: base(effect)
	{
		this.influenceLocation = influenceLocation;
	}
}
public class StateReachedTrigger : Trigger
{
	public GameConstants.Location influenceLocation;
	public GameConstants.State state;
	public bool oneshot, wasTriggered;

	public StateReachedTrigger(Effect effect, TriggerCondition condition, GameConstants.State state, GameConstants.Location influenceLocation = GameConstants.Location.Field, bool oneshot = false)
		: base(effect, condition)
	{
		this.influenceLocation = influenceLocation;
		this.state = state;
		this.oneshot = oneshot;
	}
	public StateReachedTrigger(Effect effect, GameConstants.State state, GameConstants.Location influenceLocation = GameConstants.Location.Field, bool oneshot = false)
		: base(effect)
	{
		this.influenceLocation = influenceLocation;
		this.state = state;
		this.oneshot = oneshot;
	}
}

public class CastTrigger : Trigger
{
	//public Card referrer;

	public CastTrigger(Effect effect, TriggerCondition condition/* , Card referrer */) : base(
		effect: effect,
		condition: condition
	)
	{
		//this.referrer = referrer;
	}
	public CastTrigger(Effect effect/* , Card referrer */) : base(
		effect: effect
	)
	{
		//this.referrer = referrer;
	}
}
public class RevelationTrigger : Trigger
{
	//public Card referrer;

	public RevelationTrigger(Effect effect, TriggerCondition condition/*, Card referrer*/) : base(
		effect: effect,
		condition: condition
	)
	{
		//this.referrer = referrer;
	}
	public RevelationTrigger(Effect effect/*, Card referrer*/) : base(
		effect: effect
	)
	{
		//this.referrer = referrer;
	}
}

public class GenericCastTrigger
{
	public GameConstants.Location influenceLocation;
	public TargetingCondition condition;
	public TargetingEffect effect;

	public GenericCastTrigger(TargetingEffect effect, TargetingCondition condition, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.influenceLocation = influenceLocation;
		this.condition = condition;
		this.effect = effect;
	}
	public GenericCastTrigger(TargetingEffect effect, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.influenceLocation = influenceLocation;
		this.condition = (_) => true;
		this.effect = effect;
	}
}

public class TokenCreationTrigger
{
	public GameConstants.Location influenceLocation;
	public TokenCreationCondition condition;
	public TokenCreationEffect effect;

	public TokenCreationTrigger(TokenCreationEffect effect, TokenCreationCondition condition, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.influenceLocation = influenceLocation;
		this.condition = condition;
		this.effect = effect;
	}
	public TokenCreationTrigger(TokenCreationEffect effect, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.influenceLocation = influenceLocation;
		this.condition = (_, _) => true;
		this.effect = effect;
	}
}

public class CreatureTargetingTrigger
{
	public GameConstants.Location influenceLocation;
	public CreatureTargetingCondition condition;
	public CreatureTargetingEffect effect;

	public CreatureTargetingTrigger(CreatureTargetingEffect effect, CreatureTargetingCondition condition, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.effect = effect;
		this.condition = condition;
		this.influenceLocation = influenceLocation;
	}
	public CreatureTargetingTrigger(CreatureTargetingEffect effect, GameConstants.Location influenceLocation = GameConstants.Location.Field) : this(effect, (_) => true, influenceLocation) { }
}

public class LingeringEffectInfo
{
	public static int timestampCounter;
	public int timestamp;
	public TargetingEffect effect;
	public Card referrer;
	public GameConstants.Location influenceLocation;

	public delegate void SpecificTargetingEffect<T>(T target);
	public static LingeringEffectInfo Create<T>(SpecificTargetingEffect<T> effect, T referrer, GameConstants.Location influenceLocation = GameConstants.Location.Field) where T : Card
	{
		return new LingeringEffectInfo(effect: (target) => effect((T)target), referrer: referrer, influenceLocation: influenceLocation);
	}
	private LingeringEffectInfo(TargetingEffect effect, Card referrer, GameConstants.Location influenceLocation)
	{
		this.effect = effect;
		this.referrer = referrer;
		this.influenceLocation = influenceLocation;
	}
}

public delegate bool ActivatedEffectCondition();
public class ActivatedEffectInfo
{
	public ActivatedEffectCondition condition;
	public Effect effect;
	public string name;
	public GameConstants.Location influenceLocation;
	public Card referrer;
	public int uses = 0, maxUses;

	public ActivatedEffectInfo(string name, Effect effect, ActivatedEffectCondition condition, Card referrer, int maxUses = 1, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.condition = condition;
		this.effect = effect;
		this.name = name;
		this.influenceLocation = influenceLocation;
		this.referrer = referrer;
		this.maxUses = maxUses;
	}

	public ActivatedEffectInfo(string name, Effect effect, Card referrer, int maxUses = 1, GameConstants.Location influenceLocation = GameConstants.Location.Field)
		: this(name, effect, () => true, referrer, maxUses, influenceLocation)
	{
	}
}

public delegate bool TriggerCondition();
public delegate void Effect();
public delegate void TargetingEffect(Card target);
public delegate bool TargetingCondition(Card target);
public delegate bool CreatureTargetingCondition(Creature target);
public delegate void CreatureTargetingEffect(Creature target);
public delegate void TokenCreationEffect(Creature token, Card source);
public delegate bool TokenCreationCondition(Creature token, Card source);
public delegate void RegisterCastTriggerDelegate(CastTrigger trigger, Card referrer);
public delegate void RegisterGenericCastTriggerDelegate(GenericCastTrigger trigger, Card referrer);
public delegate void RegisterTokenCreationTriggerDelegate(TokenCreationTrigger trigger, Card referrer);
public delegate void RegisterRevelationTriggerDelegate(RevelationTrigger trigger, Card referrer);
public delegate void RegisterDiscardTriggerDelegate(DiscardTrigger trigger, Card referrer);
public delegate void RegisterStateReachedTriggerDelegate(StateReachedTrigger trigger, Card referrer);
public delegate void RegisterVictoriousTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterAttackTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterDeathTriggerDelegate(CreatureTargetingTrigger trigger, Card referrer);
public delegate void RegisterGenericDeathTriggerDelegate(CreatureTargetingTrigger trigger, Card referrer);
public delegate void RegisterDealsDamageTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterLingeringEffectDelegate(LingeringEffectInfo info);
public delegate void RegisterTemporaryLingeringEffectDelegate(LingeringEffectInfo info);
public delegate void RegisterActivatedEffectDelegate(ActivatedEffectInfo info);
public delegate void CastDelegate(int player, Card card);
public delegate void DrawDelegate(int player, int amount);
public delegate Card[] GetCardsInLocationDelegate(int player);
public delegate Creature[] GetFieldUsedDelegate(int player);
public delegate Creature?[] GetWholeFieldDelegate(int player);
public delegate Card[] SelectCardsDelegate(int player, Card[] cards, int amount, string description);
public delegate void DiscardDelegate(Card card);
public delegate void DiscardAmountDelegate(int player, int amount);
public delegate void CreateTokenOnFieldDelegate(int player, int power, int life, string name, Card source);
public delegate Token CreateTokenDelegate(int player, int power, int life, string name);
public delegate Creature CreateTokenCopyDelegate(int player, Creature card);
public delegate void CreateTokenCopyOnFieldDelegate(int player, Creature card, Card source);
public delegate int GetYXTurnsAgoDelegate(int player, int turns);
public delegate void CreatureChangeStatDelegate(Creature target, int amount, Card source);
public delegate void PlayerChangeLifeDelegate(int player, int amount, Card source);
public delegate void PlayerChangeMomentumDelegate(int player, int amount);
public delegate void DestroyDelegate(Creature card);
public delegate bool AskYesNoDelegate(int player, string question);
public delegate int GetIgniteDamageDelegate(int player);
public delegate void ChangeIgniteDamageDelegate(int player, int amount);
public delegate int GetTurnDelegate();
public delegate int GetPlayerLifeDelegate(int player);
public delegate void PayLifeDelegate(int player, int amount);
public delegate Card GatherDelegate(int player, int amount);
public delegate void MoveDelegate(Creature card, int zone);
public delegate int SelectZoneDelegate(int choosingPlayer, int targetPlayer);
public delegate void MoveToHandDelegate(int player, Card card);
public delegate void MoveToFieldDelegate(int choosingPlayer, int targetPlayer, Creature card, Card source);
public delegate int GetCastCountDelegate(int player, string name);
public delegate void ReturnCardsToDeckDelegate(Card[] cards);
public delegate void RevealDelegate(int player, int damage);
public delegate Card[] GetDiscardableDelegate(int player, Card? ignore);
public delegate void RefreshAbilityDelegate(int player);
