public class InsufficientDeckSizeException : Exception
{
	public InsufficientDeckSizeException()
		: base("The player has less than 4 cards in his deck.") { }

}