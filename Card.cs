using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

public class Card
{
	public string Name, Text;
	public GameConstants.CardType CardType;
	public GameConstants.PlayerClass CardClass;
	public int uid, Life, Power, Cost, Position;
	public int BaseLife, BasePower, BaseCost;
	public GameConstants.Location Location;
	public bool IsClassAbility, CanBeClassAbility;
	public int Controller;

	public Card(GameConstants.CardType cardType,
		GameConstants.PlayerClass cardClass,
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
		this.CardType = cardType;
		this.CardClass = cardClass;
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

	private void ClearModifications()
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
}

public class Creature : Card
{
	public Creature(GameConstants.PlayerClass cardClass,
		string Name,
		string Text,
		int OriginalCost,
		int OriginalLife,
		int OriginalPower,
		GameConstants.Location OriginalLocation = GameConstants.Location.UNKNOWN,
		int OriginalPositon = 0)
	: base(cardType: GameConstants.CardType.Creature,
		cardClass: cardClass,
		Name: Name,
		Text: Text,
		OriginalCost: OriginalCost,
		OriginalLife: OriginalLife,
		OriginalPower: OriginalPower,
		OriginalLocation: OriginalLocation,
		OriginalPositon: OriginalPositon,
		IsClassAbility: false,
		CanBeClassAbility: false){}
}

public class Spell : Card
{
	public Spell(GameConstants.PlayerClass cardClass,
		string Name,
		string Text,
		int OriginalCost = 0,
		GameConstants.Location OriginalLocation = GameConstants.Location.UNKNOWN,
		bool IsClassAbility = false,
		bool CanBeClassAbility = false)
		: base(cardType: GameConstants.CardType.Spell,
			cardClass: cardClass,
			Name: Name,
			Text: Text,
			OriginalCost: OriginalCost,
			OriginalLocation: OriginalLocation,
			IsClassAbility: IsClassAbility,
			CanBeClassAbility: CanBeClassAbility){}
}