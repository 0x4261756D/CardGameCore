using CardGameUtils;
using CardGameUtils.Structs;

namespace CardGameCore;

class Field
{
	private Card[] cards = new Card[GameConstants.FIELD_SIZE];
	public Field()
	{
		
	}

	internal CardStruct?[] ToStruct()
	{
		return cards.ToList().ConvertAll(x => x?.ToStruct()).ToArray();
	}
}