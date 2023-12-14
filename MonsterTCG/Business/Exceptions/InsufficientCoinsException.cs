public class InsufficientCoinsException : Exception
{
	public InsufficientCoinsException()
		: base("Not enough money for buying a card package") { }

}