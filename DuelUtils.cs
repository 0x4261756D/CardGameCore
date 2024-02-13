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

public class LocationBasedTrigger
{
	public TriggerCondition condition;
	public Effect effect;
	public GameConstants.Location influenceLocation;

	public LocationBasedTrigger(Effect effect, TriggerCondition condition, GameConstants.Location influenceLocation)
	{
		this.influenceLocation = influenceLocation;
		this.condition = condition;
		this.effect = effect;
	}
	public LocationBasedTrigger(Effect effect, GameConstants.Location influenceLocation)
	{
		this.influenceLocation = influenceLocation;
		this.effect = effect;
		this.condition = () => true;
	}
}
public class StateReachedTrigger
{
	public GameConstants.Location influenceLocation;
	public GameConstants.State state;
	public TriggerCondition condition;
	public Effect effect;
	public bool oneshot, wasTriggered;

	public StateReachedTrigger(Effect effect, TriggerCondition condition, GameConstants.State state, GameConstants.Location influenceLocation = GameConstants.Location.Field, bool oneshot = false)
	{
		this.influenceLocation = influenceLocation;
		this.state = state;
		this.oneshot = oneshot;
		this.effect = effect;
		this.condition = condition;
	}
	public StateReachedTrigger(Effect effect, GameConstants.State state, GameConstants.Location influenceLocation = GameConstants.Location.Field, bool oneshot = false)
	{
		this.influenceLocation = influenceLocation;
		this.state = state;
		this.oneshot = oneshot;
		this.effect = effect;
		this.condition = () => true;
	}
}
public class LocationBasedTargetingTrigger
{
	public GameConstants.Location influenceLocation;
	public TargetingCondition condition;
	public TargetingEffect effect;

	public LocationBasedTargetingTrigger(TargetingEffect effect, TargetingCondition condition, GameConstants.Location influenceLocation)
	{
		this.influenceLocation = influenceLocation;
		this.condition = condition;
		this.effect = effect;
	}
	public LocationBasedTargetingTrigger(TargetingEffect effect, GameConstants.Location influenceLocation)
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

	public TokenCreationTrigger(TokenCreationEffect effect, TokenCreationCondition condition, GameConstants.Location influenceLocation)
	{
		this.influenceLocation = influenceLocation;
		this.condition = condition;
		this.effect = effect;
	}
	public TokenCreationTrigger(TokenCreationEffect effect, GameConstants.Location influenceLocation)
	{
		this.influenceLocation = influenceLocation;
		this.condition = (_, _) => true;
		this.effect = effect;
	}
}

public class CreatureTargetingTrigger(CreatureTargetingEffect effect, CreatureTargetingCondition condition, GameConstants.Location influenceLocation)
{
	public CreatureTargetingCondition condition = condition;
	public CreatureTargetingEffect effect = effect;
	public GameConstants.Location influenceLocation = influenceLocation;

	public CreatureTargetingTrigger(CreatureTargetingEffect effect, GameConstants.Location influenceLocation) : this(effect, (_) => true, influenceLocation) { }
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
	public int uses, maxUses;
	public int cardActionUid;

	public ActivatedEffectInfo(string name, Effect effect, ActivatedEffectCondition condition, Card referrer, int maxUses = 1, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.condition = condition;
		this.effect = effect;
		this.name = name;
		this.influenceLocation = influenceLocation;
		this.referrer = referrer;
		this.maxUses = maxUses;
		this.cardActionUid = DuelCore.CardActionUIDCount;
		DuelCore.CardActionUIDCount += 1;
	}

	public ActivatedEffectInfo(string name, Effect effect, Card referrer, int maxUses = 1, GameConstants.Location influenceLocation = GameConstants.Location.Field)
		: this(name, effect, () => true, referrer, maxUses, influenceLocation)
	{
	}

	public bool CanActivate(GameConstants.Location location)
	{
		return uses < maxUses && influenceLocation.HasFlag(location) && referrer.Location == location && condition();
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
public delegate void RegisterLocationBasedTargetingTriggerDelegate(LocationBasedTargetingTrigger trigger, Card referrer);
public delegate void RegisterTokenCreationTriggerDelegate(TokenCreationTrigger trigger, Card referrer);
public delegate void RegisterTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterLocationBasedTriggerDelegate(LocationBasedTrigger trigger, Card referrer);
public delegate void RegisterStateReachedTriggerDelegate(StateReachedTrigger trigger, Card referrer);
public delegate void RegisterCreatureTargetingTriggerDelegate(CreatureTargetingTrigger trigger, Card referrer);
public delegate void RegisterLingeringEffectDelegate(LingeringEffectInfo info);
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
