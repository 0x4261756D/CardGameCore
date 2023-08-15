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
	public int life, momentum;
	public List<int> discardCounts = new List<int>(), dealtDamages = new List<int>(), dealtSpellDamages = new List<int>(), brittleDeathCounts = new List<int>(), deathCounts = new List<int>();
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
			Card? card = deck.Pop();
			if(card == null)
			{
				life -= 1;
			}
			else
			{
				hand.Add(card);
			}
		}
	}

	internal void ClearCardModifications()
	{
		igniteDamage = baseIgniteDamage;
		hand.ClearCardModifications();
		field.ClearCardModifications();
	}
}