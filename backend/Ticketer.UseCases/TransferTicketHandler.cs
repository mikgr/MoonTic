using SpikeDb;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class TransferTicketHandler(TicketContractClient ticketContractClient)
{
    // todo handle time of RPC CALL
    // todo long running, better ux with pending -> failed / completed
    // todo handle already seen 
    public async Task Execute(User? currentUser, int eventId, int ticketId, string toAddress)
    {
        Console.WriteLine($"{nameof(TransferTicketHandler)} {nameof(Execute)}");
        // todo validate address cannot be self + address format
        // todo make sure we can send to any valid address
        ArgumentNullException.ThrowIfNull(toAddress);
        
        if (currentUser is null) throw new Exception("User not set");
        var contract = SpikeRepo.ReadIntId<EventContract>(eventId);
        var fromUserTicketContainer = SpikeRepo.ReadSingle<UserTicketContainer>(x => x.UserId == currentUser.Id);
        
        // todo must be transferrable to non MoonTic addresses!!
        var userWallet = SpikeRepo.ReadSingle<UserWallet>(x => x.Address.ToLower() == toAddress.ToLower());
        
        var transferResult = await ticketContractClient.OnChainTransferTicket
            (currentUser, contract, ticketId, toAddress);
        
        //contract.Transfer(ticketId, from: currentUser.Id, to: userWallet.UserId);
        
        var toUserTicketContainer = SpikeRepo.ReadSingle<UserTicketContainer>(x => x.UserId == userWallet.UserId);
        
        var transferredEvent = new TicketTransferredEvent
        {
            ContractId = contract.Id,
            ContractAddress = contract.ContractAddress,
            TicketId = ticketId,
            FromUserId = currentUser.Id,
            ToUserId = userWallet.UserId,
            TransactionHash = transferResult.receipt.TransactionHash,
            FromAddress = transferResult.receipt.From,
            ToAddress = toAddress,
            TimestampUtc = transferResult.blockTimestamp
        };
        
        fromUserTicketContainer.ApplyEvent(transferredEvent);
        toUserTicketContainer.ApplyEvent(transferredEvent);
        contract.ApplyEvent(transferredEvent);
        
        transferredEvent.SpikePersistInt();
        contract.SpikePersistInt();
        fromUserTicketContainer.SpikePersistInt();
        toUserTicketContainer.SpikePersistInt();
    }
}