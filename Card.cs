using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

public abstract class Card
{
	public bool isInitialized;
	public string Name, Text;
	public GameConstants.CardType CardType;
	public GameConstants.PlayerClass CardClass;
	public int uid;
	private int _cost, _baseController = -1;
	public int Cost
	{
		get => _cost;
		set
		{
			_cost = value;
			if(_cost < 0)
			{
				_cost = 0;
			}
		}
	}
	public readonly int BaseCost;
	public GameConstants.Location Location;
	public int Controller { get; set; }
	public int BaseController
	{
		get => _baseController;
		set
		{
			if(_baseController == -1)
			{
				_baseController = value;
			}
		}
	}
	public abstract void Init();

	public Card(GameConstants.CardType CardType,
		GameConstants.PlayerClass CardClass,
		string Name,
		string Text,
		int OriginalCost = 0,
		GameConstants.Location OriginalLocation = GameConstants.Location.UNKNOWN)
	{
		this.CardType = CardType;
		this.CardClass = CardClass;
		this.Name = Name;
		this.Text = Text;
		this.BaseCost = OriginalCost;
		this.Location = OriginalLocation;
		this.uid = DuelCore.UIDCount;
		DuelCore.UIDCount++;
		ResetToBaseState();
	}
	#region ScriptingFunctions
	public static RegisterTriggerDelegate RegisterCastTrigger = (_, _) => { };
	public static RegisterLocationBasedTargetingTriggerDelegate RegisterGenericCastTrigger = (_, _) => { };
	public static RegisterTokenCreationTriggerDelegate RegisterTokenCreationTrigger = (_, _) => { };
	public static RegisterLocationBasedTargetingTriggerDelegate RegisterGenericEntersFieldTrigger = (_, _) => { };
	public static RegisterTriggerDelegate RegisterRevelationTrigger = (_, _) => { };
	public static RegisterLocationBasedTriggerDelegate RegisterYouDiscardTrigger = (_, _) => { };
	public static RegisterTriggerDelegate RegisterDiscardTrigger = (_, _) => { };
	public static RegisterStateReachedTriggerDelegate RegisterStateReachedTrigger = (_, _) => { };
	public static RegisterTriggerDelegate RegisterVictoriousTrigger = (_, _) => { };
	public static RegisterCreatureTargetingTriggerDelegate RegisterAttackTrigger = (_, _) => { };
	public static RegisterCreatureTargetingTriggerDelegate RegisterDeathTrigger = (_, _) => { };
	public static RegisterCreatureTargetingTriggerDelegate RegisterGenericDeathTrigger = (_, _) => { };
	public static RegisterTriggerDelegate RegisterDealsDamageTrigger = (_, _) => { };
	public static RegisterLingeringEffectDelegate RegisterLingeringEffect = (_) => { };
	public static RegisterLingeringEffectDelegate RegisterTemporaryLingeringEffect = (_) => { };
	public static RegisterActivatedEffectDelegate RegisterActivatedEffect = (_) => { };
	public static GetCardsInLocationDelegate GetGrave = (_) => [];
	public static GetWholeFieldDelegate GetField = (_) => [];
	public static GetFieldUsedDelegate GetFieldUsed = (_) => [];
	public static GetCardsInLocationDelegate GetHand = (_) => [];
	public static SelectCardsDelegate SelectCards = (_, _, _, _) => [];
	public static DiscardDelegate Discard = (_) => { };
	public static DiscardAmountDelegate DiscardAmount = (_, _) => { };
	public static CreateTokenDelegate CreateToken = (_, _, _, _) => new ClientCoreDummyToken();
	public static CreateTokenOnFieldDelegate CreateTokenOnField = (_, _, _, _, _) => { };
	public static CreateTokenCopyDelegate CreateTokenCopy = (_, _) => new ClientCoreDummyToken();
	public static CreateTokenCopyOnFieldDelegate CreateTokenCopyOnField = (_, _, _) => { };
	public static GetYXTurnsAgoDelegate GetDiscardCountXTurnsAgo = (_, _) => -1;
	public static GetYXTurnsAgoDelegate GetDamageDealtXTurnsAgo = (_, _) => -1;
	public static GetYXTurnsAgoDelegate GetSpellDamageDealtXTurnsAgo = (_, _) => -1;
	public static GetYXTurnsAgoDelegate GetBrittleDeathCountXTurnsAgo = (_, _) => -1;
	public static GetYXTurnsAgoDelegate GetDeathCountXTurnsAgo = (_, _) => -1;
	public static PlayerChangeLifeDelegate PlayerChangeLife = (_, _, _) => { };
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
	public static GatherDelegate Gather = (_, _) => new ClientCoreDummyCard();
	public static MoveDelegate Move = (_, _) => { };
	public static SelectZoneDelegate SelectZone = (_, _) => -1;
	public static MoveToHandDelegate MoveToHand = (_, _) => { };
	public static MoveToFieldDelegate MoveToField = (_, _, _, _) => { };
	public static GetCastCountDelegate GetCastCount = (_, _) => -1;
	public static ReturnCardsToDeckDelegate ReturnCardsToDeck = (_) => { };
	public static RevealDelegate Reveal = (_, _) => { };
	public static GetDiscardableDelegate GetDiscardable = (_, _) => [];
	public static RefreshAbilityDelegate RefreshAbility = (_) => { };
	public static CreatureChangeStatDelegate CreatureChangeLife = (_, _, _) => { };
	public static CreatureChangeStatDelegate CreatureChangePower = (_, _, _) => { };
	#endregion ScriptingFunctions
	public virtual void ResetToBaseState()
	{
		_cost = BaseCost;
		Controller = BaseController;
	}

	public virtual bool CanBeDiscarded() => true;

	public abstract CardStruct ToStruct(bool client = false);

	public static bool operator ==(Card? first, Card? second)
	{
		if(first is null)
		{
			return true;
		}

		if(second is null)
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

		if(obj is null)
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
		return Array.ConvertAll(cards, x => x.ToStruct());
	}
}
public class ClientCoreDummyCard : Card
{
	public ClientCoreDummyCard() : base(GameConstants.CardType.UNKNOWN, GameConstants.PlayerClass.UNKNOWN, "UNINITIALIZED", "UNINITIALIZED")
	{ }
	public override void Init()
	{
	}

	public override CardStruct ToStruct(bool client = false)
	{
		return new CardStruct("DUMMY", "DUMMY", GameConstants.CardType.UNKNOWN, GameConstants.PlayerClass.UNKNOWN, -1, -1, -1, -1, -1, -1, -1, GameConstants.Location.UNKNOWN, -1, false, false, -1, -1);
	}
}
public class ClientCoreDummyToken : Token
{
	public ClientCoreDummyToken() : base("UNINITIALIZED", "UNINITIALIZED", -1, -1, -1, -1)
	{ }
	public override void Init()
	{
	}
}

public abstract partial class Creature : Card
{
	public readonly int BaseLife, BasePower;
	public int damageCap, baseDamageCap;

	private int _life, _power;
	public int Position;
	public int Life
	{
		get => _life;
		set
		{
			if(damageCap > 0 && _life - value > damageCap)
			{
				_life -= damageCap;
			}
			else
			{
				_life = value;
			}
			if(_life < 0)
			{
				_life = 0;
			}
		}
	}
	public int Power
	{
		get => _power;
		set
		{
			_power = value;
			if(_power < 0)
			{
				_power = 0;
			}
		}
	}
	public Dictionary<Keyword, int> Keywords = [];
	public Creature(GameConstants.PlayerClass CardClass,
		string Name,
		string Text,
		int OriginalCost,
		int OriginalLife,
		int OriginalPower)
	: base(CardType: GameConstants.CardType.Creature,
		CardClass: CardClass,
		Name: Name,
		Text: Text,
		OriginalCost: OriginalCost)
	{
		BaseLife = OriginalLife;
		BasePower = OriginalPower;
		ResetToBaseState();
	}
	public int CalculateMovementCost()
	{
		return 1 + Keywords.GetValueOrDefault(Keyword.Colossal, 0);
	}
	public void RegisterKeyword(Keyword keyword, int amount = 0)
	{
		Keywords[keyword] = amount;
	}

	public override void ResetToBaseState()
	{
		base.ResetToBaseState();
		_life = BaseLife;
		_power = BasePower;
		damageCap = baseDamageCap;
	}

	public override CardStruct ToStruct(bool client = false)
	{
		StringBuilder text = new();
		if(client)
		{
			_ = text.Append(Text);
		}
		else
		{
			if(Keywords.Count > 0)
			{
				foreach(var keyword in Keywords)
				{
					_ = text.Append($"[{keyword.Key}] ");
					if(keyword.Value != 0)
					{
						if(keyword.Key == Keyword.Colossal)
						{
							_ = text.Append('+');
						}
						_ = text.Append($"{keyword.Value}");
					}
					_ = text.Append('\n');
				}
			}
			_ = text.Append(KeywordRegex().Replace(Text, ""));
		}
		return new CardStruct(name: Name,
			text: text.ToString(),
			card_type: CardType,
			card_class: CardClass,
			uid: uid, life: Life, power: Power, cost: Cost,
			base_cost: BaseCost, base_life: BaseLife, base_power: BasePower,
			location: Location, position: Position,
			is_class_ability: false,
			can_be_class_ability: false,
			controller: Controller,
			base_controller: BaseController);
	}

	[GeneratedRegex(@"(?m:^\[.+\](?: \+?\d+)?$)\n?")]
	private static partial Regex KeywordRegex();
}

public abstract class Spell(GameConstants.PlayerClass CardClass,
	string Name,
	string Text,
	int OriginalCost = 0,
	GameConstants.Location OriginalLocation = GameConstants.Location.UNKNOWN,
	bool IsClassAbility = false,
	bool CanBeClassAbility = false) : Card(CardType: GameConstants.CardType.Spell,
		CardClass: CardClass,
		Name: Name,
		Text: Text,
		OriginalCost: OriginalCost,
		OriginalLocation: OriginalLocation)
{
	public bool IsClassAbility = IsClassAbility, CanBeClassAbility = CanBeClassAbility;

	public override CardStruct ToStruct(bool client = false)
	{
		return new CardStruct(name: Name,
			text: Text,
			card_type: CardType,
			card_class: CardClass,
			uid: uid, life: 0, power: 0, cost: Cost,
			base_cost: BaseCost, base_life: 0, base_power: 0,
			location: Location, position: 0,
			is_class_ability: IsClassAbility,
			can_be_class_ability: CanBeClassAbility,
			controller: Controller,
			base_controller: BaseController);
	}
}

public abstract class Quest(string Name, string Text, int ProgressGoal, GameConstants.PlayerClass CardClass) : Card(
	CardType: GameConstants.CardType.Quest,
	CardClass: CardClass,
	Name: Name,
	Text: Text)
{
	private int _progress;
	public int Progress
	{
		get => _progress;
		set => _progress = Math.Min(value, Goal);
	}
	public readonly int Goal = ProgressGoal;

	public abstract void Reward();

	public override CardStruct ToStruct(bool client = false)
	{
		return new CardStruct(name: Name,
			text: Text,
			card_type: CardType,
			card_class: CardClass,
			uid: uid, life: 0, power: 0, cost: Goal,
			base_cost: BaseCost, base_life: 0, base_power: 0,
			location: Location, position: Progress,
			is_class_ability: false,
			can_be_class_ability: false,
			controller: Controller,
			base_controller: BaseController);
	}
}

public class Token : Creature
{
	public Token(string Name,
		string Text,
		int OriginalCost,
		int OriginalLife,
		int OriginalPower,
		int OriginalController) : base(
			Name: Name,
			Text: Text,
			OriginalCost: OriginalCost,
			OriginalLife: OriginalLife,
			OriginalPower: OriginalPower,
			CardClass: GameConstants.PlayerClass.All
		)
	{
		BaseController = OriginalController;
		RegisterKeyword(Keyword.Token);
		ResetToBaseState();
	}
	public override void Init()
	{
	}
}
