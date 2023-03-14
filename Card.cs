using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

public abstract class Card
{
	public string Name, Text;
	public GameConstants.CardType CardType;
	public GameConstants.PlayerClass CardClass;
	public int uid, Position;
	private int _life, _power, _cost;
	public int Life
	{
		get => _life;
		set
		{
			_life = value;
			if(_life < 0) _life = 0;
		}
	}
	public int Power
	{
		get => _power;
		set
		{
			_power = value;
			if(_power < 0) _power = 0;
		}
	}
	public int Cost
	{
		get => _cost;
		set
		{
			_cost = value;
			if(_cost < 0) _cost = 0;
		}
	}
	public int BaseLife, BasePower, BaseCost;
	public GameConstants.Location Location;
	public bool IsClassAbility, CanBeClassAbility;
	public int Controller;
	public bool CanMove = true;
	public Dictionary<Keyword, int> Keywords = new Dictionary<Keyword, int>();
	public abstract void Init();

	public int CalculateMovementCost()
	{
		return 1 + Keywords.GetValueOrDefault(Keyword.Colossal, 0);
	}

	public void RegisterKeyword(Keyword keyword, int amount = 0)
	{
		Keywords[keyword] = amount;
	}

	public Card(GameConstants.CardType CardType,
		GameConstants.PlayerClass CardClass,
		string Name,
		string Text,
		bool IsClassAbility,
		bool CanBeClassAbility,
		int Controller = 0,
		int OriginalCost = 0,
		int OriginalLife = 0,
		int OriginalPower = 0,
		GameConstants.Location OriginalLocation = GameConstants.Location.UNKNOWN,
		int OriginalPositon = 0)
	{
		this.CardType = CardType;
		this.CardClass = CardClass;
		this.Name = Name;
		this.Text = Text;
		this.BaseLife = OriginalLife;
		this.BasePower = OriginalPower;
		this.BaseCost = OriginalCost;
		this.Position = OriginalPositon;
		this.Location = OriginalLocation;
		this.IsClassAbility = IsClassAbility;
		this.Controller = Controller;
		this.CanBeClassAbility = CanBeClassAbility;
		ClearModifications();
	}

	public RegisterCastTriggerDelegate RegisterCastTrigger = (_, _) => { };
	public RegisterGenericCastTriggerDelegate RegisterGenericCastTrigger = (_, _) => { };
	public RegisterRevelationTriggerDelegate RegisterRevelationTrigger = (_, _) => { };
	public RegisterYouDiscardTriggerDelegate RegisterYouDiscardTrigger = (_, _) => { };
	public RegisterStateReachedTriggerDelegate RegisterStateReachedTrigger = (_, _) => { };
	public RegisterVictoriousTriggerDelegate RegisterVictoriousTrigger = (_, _) => { };
	public RegisterLingeringEffectDelegate RegisterLingeringEffect = (_) => { };
	public RegisterTemporaryLingeringEffectDelegate RegisterTemporaryLingeringEffect = (_) => { };
	public GetCardsInLocationDelegate GetGrave = (_) => new Card[0];
	public GetWholeFieldDelegate GetField = (_) => new Card?[0];
	public GetCardsInLocationDelegate GetFieldUsed = (_) => new Card[0];
	public GetBothFieldsUsedDelegate GetBothFieldsUsed = () => new Card[0];
	public GetCardsInLocationDelegate GetHand = (_) => new Card[0];
	public SelectCardsDelegate SelectCards = (_, _, _, _) => new Card[0];
	public DiscardDelegate Discard = (_) => { };
	public CreateTokenDelegate CreateToken = (_, _, _, _) => new ClientCoreDummyCard();
	public CreateTokenCopyDelegate CreateTokenCopy = (_, _) => new ClientCoreDummyCard();
	public GetDiscardCountXTurnsAgoDelegate GetDiscardCountXTurnsAgo = (_, _) => -1;
	public GetDamageDealtXTurnsAgoDelegate GetDamageDealtXTurnsAgo = (_, _) => -1;
	public PlayerChangeLifeDelegate PlayerChangeLife = (_, _) => { };
	public PlayerChangeMomentumDelegate PlayerChangeMomentum = (_, _) => { };
	public CastDelegate Cast = (_, _) => { };
	public DrawDelegate Draw = (_, _) => { };
	public DestroyDelegate Destroy = (_) => { };
	public AskYesNoDelegate AskYesNo = (_, _) => false;
	public GetIgniteDamageDelegate GetIgniteDamage = (_) => -1;
	public ChangeIgniteDamageDelegate ChangeIgniteDamage = (_, _) => { };

	public void ClearModifications()
	{
		Life = BaseLife;
		Power = BasePower;
		Cost = BaseCost;
	}

	public virtual CardStruct ToStruct()
	{
		return new CardStruct(name: Name,
			text: Text,
			card_type: CardType,
			card_class: CardClass,
			uid: uid, life: Life, power: Power, cost: Cost,
			location: Location, position: Position,
			is_class_ability: IsClassAbility,
			can_be_class_ability: CanBeClassAbility,
			controller: Controller);
	}

	public static bool operator ==(Card? first, Card? second)
	{
		if(ReferenceEquals(first, null))
		{
			return true;
		}

		if(ReferenceEquals(second, null))
		{
			return false;
		}

		return first.uid == second.uid;
	}

	public static bool operator !=(Card? first, Card? second)
	{
		return !(first == second);
	}

	public override bool Equals(object? obj)
	{
		if(ReferenceEquals(this, obj))
		{
			return true;
		}

		if(ReferenceEquals(obj, null))
		{
			return false;
		}

		return obj.GetType() == GetType() && (Card)obj == this;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal static CardStruct[] ToStruct(Card[] cards)
	{
		return cards.ToList().ConvertAll(x => x.ToStruct()).ToArray();
	}
}

public class ClientCoreDummyCard : Card
{
	public ClientCoreDummyCard() : base(GameConstants.CardType.UNKNOWN, GameConstants.PlayerClass.UNKNOWN, "UNINITIALIZED", "UNINITIALIZED", false, false)
	{ }
	public override void Init()
	{
	}
}

public abstract class Creature : Card
{
	public Creature(GameConstants.PlayerClass CardClass,
		string Name,
		string Text,
		int OriginalCost,
		int OriginalLife,
		int OriginalPower
		)
	: base(CardType: GameConstants.CardType.Creature,
		CardClass: CardClass,
		Name: Name,
		Text: Text,
		OriginalCost: OriginalCost,
		OriginalLife: OriginalLife,
		OriginalPower: OriginalPower,
		IsClassAbility: false,
		CanBeClassAbility: false)
	{ }
}

public abstract class Spell : Card
{
	public Spell(GameConstants.PlayerClass CardClass,
		string Name,
		string Text,
		int OriginalCost = 0,
		GameConstants.Location OriginalLocation = GameConstants.Location.UNKNOWN,
		bool IsClassAbility = false,
		bool CanBeClassAbility = false)
		: base(CardType: GameConstants.CardType.Spell,
			CardClass: CardClass,
			Name: Name,
			Text: Text,
			OriginalCost: OriginalCost,
			OriginalLocation: OriginalLocation,
			IsClassAbility: IsClassAbility,
			CanBeClassAbility: CanBeClassAbility)
	{ }
}

public abstract class Quest : Card
{
	public int Progress
	{
		get => this.Position;
		set => this.Position = value;
	}
	public int Goal
	{
		get => this.Cost;
	}

	public abstract void Reward();

	public Quest(string Name, string Text, int ProgressGoal, GameConstants.PlayerClass CardClass) : base(
		CardType: GameConstants.CardType.Quest,
		CardClass: CardClass,
		IsClassAbility: false,
		CanBeClassAbility: false,
		Name: Name,
		Text: Text,
		// Position = Progress, Cost = Goal
		OriginalPositon: 0,
		OriginalCost: ProgressGoal
	)
	{ }
}

public class Token : Creature
{
	public Token(string Name,
		string Text,
		int OriginalCost,
		int OriginalLife,
		int OriginalPower) : base(
			Name: Name,
			Text: Text,
			OriginalCost: OriginalCost,
			OriginalLife: OriginalLife,
			OriginalPower: OriginalPower,
			CardClass: GameConstants.PlayerClass.All
		)
	{ }
	public override void Init()
	{
		RegisterKeyword(Keyword.Token);
	}
}