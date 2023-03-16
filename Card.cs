using System.Text;
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

	public static RegisterCastTriggerDelegate RegisterCastTrigger = (_, _) => { };
	public static RegisterGenericCastTriggerDelegate RegisterGenericCastTrigger = (_, _) => { };
	public static RegisterRevelationTriggerDelegate RegisterRevelationTrigger = (_, _) => { };
	public static RegisterDiscardTriggerDelegate RegisterYouDiscardTrigger = (_, _) => { };
	public static RegisterDiscardTriggerDelegate RegisterDiscardTrigger = (_, _) => { };
	public static RegisterStateReachedTriggerDelegate RegisterStateReachedTrigger = (_, _) => { };
	public static RegisterVictoriousTriggerDelegate RegisterVictoriousTrigger = (_, _) => { };
	public static RegisterAttackTriggerDelegate RegisterAttackTrigger = (_, _) => { };
	public static RegisterDeathTriggerDelegate RegisterDeathTrigger = (_, _) => { };
	public static RegisterGenericDeathTriggerDelegate RegisterGenericDeathTrigger = (_, _) => { };
	public static RegisterLingeringEffectDelegate RegisterLingeringEffect = (_) => { };
	public static RegisterTemporaryLingeringEffectDelegate RegisterTemporaryLingeringEffect = (_) => { };
	public static RegisterActivatedEffectDelegate RegisterActivatedEffect = (_) => { };
	public static GetCardsInLocationDelegate GetGrave = (_) => new Card[0];
	public static GetWholeFieldDelegate GetField = (_) => new Card?[0];
	public static GetCardsInLocationDelegate GetFieldUsed = (_) => new Card[0];
	public static GetCardsInLocationDelegate GetHand = (_) => new Card[0];
	public static SelectCardsDelegate SelectCards = (_, _, _, _) => new Card[0];
	public static DiscardDelegate Discard = (_) => { };
	public static DiscardAmountDelegate DiscardAmount = (_, _) => { };
	public static CreateTokenDelegate CreateToken = (_, _, _, _) => new ClientCoreDummyCard();
	public static CreateTokenCopyDelegate CreateTokenCopy = (_, _) => new ClientCoreDummyCard();
	public static GetYXTurnsAgoDelegate GetDiscardCountXTurnsAgo = (_, _) => -1;
	public static GetYXTurnsAgoDelegate GetDamageDealtXTurnsAgo = (_, _) => -1;
	public static GetYXTurnsAgoDelegate GetBrittleDeathCountXTurnsAgo = (_, _) => -1;
	public static PlayerChangeLifeDelegate PlayerChangeLife = (_, _) => { };
	public static PlayerChangeMomentumDelegate PlayerChangeMomentum = (_, _) => { };
	public static CastDelegate Cast = (_, _) => { };
	public static DrawDelegate Draw = (_, _) => { };
	public static DestroyDelegate Destroy = (_) => { };
	public static AskYesNoDelegate AskYesNo = (_, _) => false;
	public static GetIgniteDamageDelegate GetIgniteDamage = (_) => -1;
	public static ChangeIgniteDamageDelegate ChangeIgniteDamage = (_, _) => { };
	public static ChangeIgniteDamageDelegate ChangeIgniteDamageTemporary = (_, _) => { };
	public static GetTurnDelegate GetTurn = () => -1;
	public static GetPlayerLifeDelegate GetPlayerLife = (_) => -1;
	public static PayLifeDelegate PayLife = (_, _) => { };
	public static GatherDelegate Gather = (_, _) => { };
	public static MoveDelegate Move = (_, _) => { };
	public static SelectZoneDelegate SelectZone = (_, _) => -1;
	public static AddToHandDelegate AddToHand = (_, _) => { };
	public static GetCastCountDelegate GetCastCount = (_, _) => -1;

	public void ClearModifications()
	{
		Life = BaseLife;
		Power = BasePower;
		Cost = BaseCost;
	}

	public virtual CardStruct ToStruct()
	{
		StringBuilder text = new StringBuilder();
		if(Keywords.Count > 0)
		{
			foreach (var keyword in Keywords)
			{
				text.Append($"[{keyword.Key}] ");
				if(keyword.Value != 0)
				{
					if(keyword.Key == Keyword.Colossal)
					{
						text.Append('+');
					}
					text.Append($"{keyword.Value}");
				}
				text.AppendLine();
			}
		}
		text.Append(Text);
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
		int OriginalPower,
		int Controller) : base(
			Name: Name,
			Text: Text,
			OriginalCost: OriginalCost,
			OriginalLife: OriginalLife,
			OriginalPower: OriginalPower,
			CardClass: GameConstants.PlayerClass.All
		)
	{
		this.Controller = Controller;
	}
	public override void Init()
	{
		RegisterKeyword(Keyword.Token);
	}
}