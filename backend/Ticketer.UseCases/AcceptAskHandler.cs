using Ticketer.Model;

namespace Ticketer.UseCases;


// todo this is a saga - we must handle step by step roll back with compensating transactions 
public class AcceptAskHandler(
    TimeProvider timeProvider,
    IRepository repo,
    TicketContractClient ticketContractClient,
    StableCoinContractClient stableCoinContractClient)
{
    public async Task Execute(string contractAddress, int ticketId, int priceInFiat, User byUser)
    {
        if (string.IsNullOrEmpty(contractAddress)) throw new ArgumentException("Contract address cannot be null or empty", nameof(contractAddress));
        if (ticketId <= 0) throw new ArgumentException("Ticket id must be greater than 0", nameof(ticketId));
        if (priceInFiat <= 0) throw new ArgumentException("Price must be greater than 0", nameof(priceInFiat));
        if (byUser is null) throw new ArgumentException("User cannot be null", nameof(byUser));
        
        var ask = await repo.FindAsk(contractAddress, ticketId);
        if (ask is null) throw new DomainInvariant("Ask not found");
        if (ask.Price != priceInFiat) throw new DomainInvariant("Ask price does not match");
        
        // - [x] holder/seller: Create Ask
        
        // - [ ] TODO buyer: Pay fiat to moontic - for the POC We'll just log that this as it went through
        
        // - [ ] moontic: Fund buyer address with usdc
        var buyerWallet = await repo.LoadUserWallet(byUser.Id);
        var contract = await repo.LoadContractBy(contractAddress);
        var (receipt, blockTimestamp) = await stableCoinContractClient.TransferFromLiquidityAccount(
            symbol: contract.PaymentStableCoinSymbol, amountInFiat: priceInFiat, to: buyerWallet.Address);
        
        Console.WriteLine($"Buyer account funded with {contract.PaymentStableCoinSymbol} {priceInFiat} to {buyerWallet.Address} {receipt.TransactionHash}");
        
        // - [ ] moontic-for-buyer: approve ticket-contract to pull usdc from buyer address
        var (approveReceipt, approveBlockTimestamp) = await stableCoinContractClient.Approve(
                buyerWallet, contract.ContractAddress, contract.PaymentStableCoinSymbol, priceInFiat);
        
        Console.WriteLine($"Approved ticket-contract to pull {contract.PaymentStableCoinSymbol} from buyer address {approveReceipt.TransactionHash}");
        
        
        // - [ ] moontic-for-buyer: accept Ask
        var (acceptReceipt, acceptBlockTimestamp) = await ticketContractClient.OnChainAcceptAsk(
            byUser, ticketId, contract);
        
        //TODO ^^ this could fail if some else buys the ticket before. In that case we must roll back the funding and approval.
        
        Console.WriteLine($"Accepted Ask {acceptReceipt.TransactionHash}");
        
        var purchaseEvent = new TicketPurchasedEvent
        {
            ContractAddress = contractAddress,
            TimestampUtc = timeProvider.GetUtcNow().UtcDateTime,
            OwnerId = byUser.Id,
            SellerId = ask.UserId,
            EventContractId = contract.Id,
            TicketId = ticketId,
            TransactionHash = acceptReceipt.TransactionHash,
            ToAddress = acceptReceipt.From, // todo test this the ticket lands on the tx.From <- so that is the to
            TicketPrice = priceInFiat,
            PurchaseType = nameof(PurchaseType.Secondary)
        };
        
        // todo update seller and buyer ticket containers
        var sellerTicketContainer = await repo.LoadUserTicketContainer(ask.UserId);
        var buyerTicketContainer = await repo.LoadUserTicketContainer(byUser.Id);
        
        sellerTicketContainer.ApplyEvent(purchaseEvent);
        buyerTicketContainer.ApplyEvent(purchaseEvent);
        
        var writeAsk = repo.CreateTransactWrite<TicketAsk>();
        writeAsk.AddDeleteItem(ask);
        
        var writeTicketContainer = repo.CreateTransactWrite<UserTicketContainerState>();
        writeTicketContainer.AddSaveItem(sellerTicketContainer.GetState());
        writeTicketContainer.AddSaveItem(buyerTicketContainer.GetState());
        
        var writeEvent = repo.CreateTransactWrite<TicketPurchasedEvent>();
        writeEvent.AddSaveItem(purchaseEvent);
        
        var transaction = repo.CreateMultiTableTransactWrite(writeAsk, writeTicketContainer, writeEvent);
        await transaction.ExecuteAsync();
        
        // TODO Settle
        // - [ ] ticket-contract: Settle, pull usdc from buyer address - send to holder/seller, transfer ticket to buyer
        // - [ ] moontic-for-buyer: Settle, send usdc to moontic system address, and payout seller in fiat
        // - [ ] moontic: Notify seller that the ticket is sold
        // todo make sure all fiat events can be seen under user profile 
    }
}