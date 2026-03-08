using Ticketer.Model;

namespace Ticketer.UseCases;

public class AcceptAskHandler
{
    public  Task Execute(string contractAddress, int ticketId, int price, User user)
    {
        // - [x] holder/seller: Create Ask
        // - [ ] buyer: Pay fiat to moontic
        // - [ ] moontic: Fund buyer address with usdc
        // - [ ] moontic-for-buyer: approve ticket-contract to pull usdc from buyer address
        // - [ ] moontic-for-buyer: accept Ask
        // - [ ] ticket-contract: pull usdc from buyer address - send to holder/seller, transfer ticket to buyer
        // - [ ] moontic-for-buyer: send usdc to moontic system address, and payout seller in fiat
        // - [ ] moontic: Notify seller that the ticket is sold
        // todo make sure all fiat events can be seen under user profile 
        throw new NotImplementedException();
    }
}