using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Player
{
	public Deck deck;
	public Grave grave = new Grave();
	public Field field = new Field();
	public Hand hand = new Hand();
	public string id;
	public int number;
	public string name;
	public bool passed;
	public GameConstants.PlayerClass playerClass;
	public Card ability;
	public Quest quest;
	public int life, progress, momentum;
	public List<int> discardCounts = new List<int>(), dealtDamages = new List<int>(), brittleDeathCounts = new List<int>();
	public int baseIgniteDamage = 1, igniteDamage;
	public Dictionary<string, int> castCounts = new Dictionary<string, int>();
	public Player(CoreConfig.PlayerConfig config, int number, Deck deck, GameConstants.PlayerClass playerClass, Card ability, Quest quest)
	{
		this.deck = deck;
		this.id = config.id;
		this.name = config.name;
		this.passed = false;
		this.playerClass = playerClass;
		this.ability = ability;
		this.ability.Location = GameConstants.Location.Ability;
		this.quest = quest;
		this.quest.Location = GameConstants.Location.Quest;
		this.number = number;
	}

	internal void Draw(int amount)
	{
		for(int i = 0; i < amount; i++)
		{
			hand.Add(deck.Pop());
		}
	}
	internal void CastCreature(Card card, int zone)
	{
		field.Add(card, zone);
	}
	internal void CastSpell(Card card)
	{
		grave.Add(card);
	}

	internal void ClearCardModifications()
	{
		igniteDamage = baseIgniteDamage;
		hand.ClearCardModifications();
		field.ClearCardModifications();
	}

	internal void Discard(Card card)
	{
		hand.Remove(card);
		grave.Add(card);
	}
	internal void Destroy(Card card)
	{
		switch(card.Location)
		{
			case GameConstants.Location.Field:
			{
				field.Remove(card);
				grave.Add(card);
			}
			break;
			default:
				throw new Exception($"Destroying a card at {card.Location} is not supported");
		}
	}
}