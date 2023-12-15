public class DealNotFoundException : Exception
{
	public DealNotFoundException()
		: base("The provided deal ID was not found.") { }

}