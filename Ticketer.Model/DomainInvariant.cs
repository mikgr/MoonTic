namespace Ticketer.Model;

public class DomainInvariant : Exception
{
    public DomainInvariant() { }
    public DomainInvariant(string message) : base(message) { }
    public DomainInvariant(string message, Exception innerException) : base(message, innerException) { }
}