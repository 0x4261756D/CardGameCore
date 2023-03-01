using CardGameUtils;

namespace CardGameCore;

public enum Keyword
{
	Colossal,
	Brittle,
	Token,
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

public delegate bool TriggerCondition();
public delegate void Effect();

public delegate void RegisterCastTriggerDelegate(Effect effect, TriggerCondition condition, Card referrer);
public delegate void RegisterLingeringEffectDelegate(LingeringEffectInfo info);
public delegate Card?[] GetFieldDelegate(int player);
public delegate Card[] GetHandDelegate(int player);
public delegate Card[] SelectCardsDelegate(int player, Card[] cards, int amount, string description);
public delegate void DiscardDelegate(Card card);
public delegate void CreateTokenDelegate(int player, int power, int life, string name);
public delegate int GetDiscardCountThisTurnDelegate(int player);

public class LingeringEffectInfo
{
	public Effect effect;
	public Card referrer;
	public GameConstants.Location influenceLocation;

	public LingeringEffectInfo(Effect effect, Card referrer, GameConstants.Location influenceLocation = GameConstants.Location.Field)
	{
		this.effect = effect;
		this.referrer = referrer;
		this.influenceLocation = influenceLocation;
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