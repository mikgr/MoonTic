using Ticketer.Model;

namespace Ticketer;

public class PrintSecretHandler
{
    public void Execute(User? currentUser, int contractId, int ticketId)
    {
        if (currentUser is null) throw new Exception("Current user not set");
        var secret = currentUser.GetSecret(contractId, ticketId);
        Console.WriteLine(secret);
    }
}