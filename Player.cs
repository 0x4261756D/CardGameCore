using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Player
{
	public Deck deck;
	public Grave grave = new Grave();
	public Field field = new Field();
	public string id;
	public int number;
	public string name;
	public bool passed;
	public GameConstants.PlayerClass playerClass;
	public Card ability, quest;
	public Player(CoreConfig.PlayerConfig config, int number, Deck deck, GameConstants.PlayerClass playerClass, Card ability, Card quest)
	{
		this.deck = deck;
		this.id = config.id;
		this.name = config.name;
		this.passed = false;
		this.playerClass = playerClass;
		this.ability = ability;
		this.quest = quest;
		this.number = number;
	}
}