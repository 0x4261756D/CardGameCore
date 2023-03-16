using CardGameUtils;

namespace CardGameCore;

public enum Keyword
{
	Colossal,
	Brittle,
	Token,
	Decaying,
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

public class GenericCastTrigger : Trigger
{
	public GameConstants.Location influenceLocation;
	public new GenericCastTriggerCondition condition;
	public new GenericCastTriggerEffect effect;

	public GenericCastTrigger(GenericCastTriggerEffect effect, GenericCastTriggerCondition condition, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.influenceLocation = influenceLocation;
		this.condition = condition;
		this.effect = effect;
	}
	public GenericCastTrigger(GenericCastTriggerEffect effect, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.influenceLocation = influenceLocation;
		this.condition = (_) => true;
		this.effect = effect;
	}
}

public class GenericDeathTrigger : Trigger
{
	public new GenericDeathTriggerCondition condition;
	public new GenericDeathTriggerEffect effect;

	public GenericDeathTrigger(GenericDeathTriggerEffect effect, GenericDeathTriggerCondition condition)
	{
		this.effect = effect;
		this.condition = condition;
	}
	public GenericDeathTrigger(GenericDeathTriggerEffect effect) : this(effect, (_) => true) { }
}

public delegate bool TriggerCondition();
public delegate void Effect();
public delegate bool GenericCastTriggerCondition(Card castCard);
public delegate void GenericCastTriggerEffect(Card castCard);
public delegate bool GenericDeathTriggerCondition(Card destroyedCard);
public delegate void GenericDeathTriggerEffect(Card destroyedCard);
public delegate void TargetingEffect(Card target);

public delegate void RegisterCastTriggerDelegate(CastTrigger trigger, Card referrer);
public delegate void RegisterGenericCastTriggerDelegate(GenericCastTrigger trigger, Card referrer);
public delegate void RegisterRevelationTriggerDelegate(RevelationTrigger trigger, Card referrer);
public delegate void RegisterDiscardTriggerDelegate(DiscardTrigger trigger, Card referrer);
public delegate void RegisterStateReachedTriggerDelegate(StateReachedTrigger trigger, Card referrer);
public delegate void RegisterVictoriousTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterAttackTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterDeathTriggerDelegate(Trigger trigger, Card referrer);
public delegate void RegisterGenericDeathTriggerDelegate(GenericDeathTrigger trigger, Card referrer);
public delegate void RegisterLingeringEffectDelegate(LingeringEffectInfo info);
public delegate void RegisterTemporaryLingeringEffectDelegate(LingeringEffectInfo info);
public delegate void RegisterActivatedEffectDelegate(ActivatedEffectInfo info);
public delegate void CastDelegate(int player, Card card);
public delegate void DrawDelegate(int player, int amount);
public delegate Card[] GetCardsInLocationDelegate(int player);
public delegate Card?[] GetWholeFieldDelegate(int player);
public delegate Card[] SelectCardsDelegate(int player, Card[] cards, int amount, string description);
public delegate void DiscardDelegate(Card card);
public delegate void DiscardAmountDelegate(int player, int amount);
public delegate Card CreateTokenDelegate(int player, int power, int life, string name);
public delegate Card CreateTokenCopyDelegate(int player, Card card);
public delegate int GetYXTurnsAgoDelegate(int player, int turns);
public delegate void PlayerChangeLifeDelegate(int player, int amount);
public delegate void PlayerChangeMomentumDelegate(int player, int amount);
public delegate void DestroyDelegate(Card card);
public delegate bool AskYesNoDelegate(int player, string question);
public delegate int GetIgniteDamageDelegate(int player);
public delegate void ChangeIgniteDamageDelegate(int player, int amount);
public delegate int GetTurnDelegate();
public delegate int GetPlayerLifeDelegate(int player);
public delegate void PayLifeDelegate(int player, int amount);
public delegate Card GatherDelegate(int player, int amount);
public delegate void MoveDelegate(Card card, int zone);
public delegate int SelectZoneDelegate(int choosingPlayer, int targetPlayer);
public delegate void AddToHandDelegate(int player, Card card);
public delegate int GetCastCountDelegate(int player, string name);

public class LingeringEffectInfo
{
	public TargetingEffect effect;
	public Card referrer;
	public GameConstants.Location influenceLocation;

	public LingeringEffectInfo(TargetingEffect effect, Card referrer, GameConstants.Location influenceLocation = GameConstants.Location.Field)
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

public class EffectChain
{
	private class ChainLink
	{
		public Effect effect;
		public int uid;

		public ChainLink(Effect effect, int uid)
		{
			this.effect = effect;
			this.uid = uid;
		}
	}
	private Stack<ChainLink> links;
	public Card?[] playersLastCard;

	public EffectChain(int playerCount)
	{
		links = new Stack<ChainLink>();
		playersLastCard = new Card?[playerCount];
	}

	public void Push(Card initiator, Effect effect)
	{
		links.Push(new ChainLink(effect, initiator.uid));
		playersLastCard[initiator.Controller] = initiator;
	}

	public bool Pop()
	{
		ChainLink? link;
		bool isEmpty = links.TryPop(out link);
		if(!isEmpty)
		{
			return false;
		}
		link!.effect();
		return true;
	}

	public int Count()
	{
		return links.Count;
	}
}