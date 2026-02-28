using System.Runtime.InteropServices.JavaScript;
using SpikeDb;
using Ticketer.Model;

namespace Ticketer.Test
{
    public class UserTicketContainerTest
    {
        [Fact]
        public void When_ticket_is_transferred_it_is_removed_from_the_container()
        {
            SpikeRepo.Truncate<UserTicketContainer>();
            
            var userTicketContainer = new UserTicketContainer { Id = -1, UserId = 1 };

            userTicketContainer.ApplyEvent(new TicketPurchasedEvent
            {
                Id = 1,
                TimestampUtc = DateTime.UtcNow,
                OwnerId = 1,
                EventContractId = 1,
                TicketId = 1,
                TransactionHash = "123",
                ContractAddress = "0xasdf",
                ToAddress = "0x1234"
            }).SpikePersistInt();

            var actual = userTicketContainer.GetAllTickets();
            Assert.Single(actual);
            
            userTicketContainer.ApplyEvent(new TicketTransferredEvent
            {
                Id = 2,
                FromUserId = 1,
                ToUserId = 2,
                ContractId = 1,
                TicketId = 1,
                TransactionHash = "hash",
                FromAddress = "address",
                ToAddress = "address to",
                TimestampUtc = DateTime.UtcNow,
                ContractAddress = "contract address"
            }).SpikePersistInt();
            
            var actual2 = userTicketContainer.GetAllTickets();
            Assert.Empty(actual2);
        }
        
        [Fact]
        public void When_ticket_is_checked_in_it_is_removed_from_base_state_and_added_to_check_in_state()
        {
            SpikeRepo.Truncate<UserTicketContainer>();
            
            var userTicketContainer = new UserTicketContainer { Id = -1, UserId = 1 };

            userTicketContainer.ApplyEvent(new TicketPurchasedEvent
            {
                Id = 1,
                TimestampUtc = DateTime.UtcNow,
                OwnerId = 1,
                EventContractId = 1,
                TicketId = 1,
                TransactionHash = "123",
                ContractAddress = "0xasdf",
                ToAddress = "0x1234"
            }).SpikePersistInt();

            var actual = userTicketContainer.GetAllTickets();
            Assert.Single(actual);
            
            userTicketContainer.ApplyEvent(new TicketCheckedInEvent
            {
                Id = 2,
                EventContractId = 1,
                TicketId = 1,
                UserId = 1,
                TimestampUtc = DateTime.UtcNow,
                ContractAddress = "contract address",
                TransactionHash = "transaction hash",
                Address = "address",
                CheckInSecretHash = "secret hash"
            }).SpikePersistInt();
            
            var actual2 = userTicketContainer.GetAllTickets().Where(x => !x.IsCheckedIn);
            Assert.Empty(actual2);
            
            var actual3 = userTicketContainer.GetAllTickets().Where(x => x.IsCheckedIn);
            Assert.Single(actual3);
        }
        
        [Fact]
        public void When_ticket_is_checked_out_it_is_removed_from_check_in_state()
        {
            SpikeRepo.Truncate<UserTicketContainer>();
            
            var userTicketContainer = new UserTicketContainer { Id = -1, UserId = 1 };

            userTicketContainer.ApplyEvent(new TicketPurchasedEvent
            {
                Id = 1,
                TimestampUtc = DateTime.UtcNow,
                OwnerId = 1,
                EventContractId = 1,
                TicketId = 1,
                TransactionHash = "123",
                ContractAddress = "0xasdf",
                ToAddress = "0x1234"
            }).SpikePersistInt();
            
            userTicketContainer.ApplyEvent(new TicketCheckedInEvent
            {
                Id = 2,
                EventContractId = 1,
                TicketId = 1,
                UserId = 1,
                TimestampUtc = default,
                ContractAddress = "contract address",
                TransactionHash = "transaction hash",
                Address = "address",
                CheckInSecretHash = "secret hash"
            }).SpikePersistInt();

            userTicketContainer.ApplyEvent(new TicketCheckedOutEvent
            {
                Id = 3,
                EventContractId = 1,
                TicketId = 1,
                UserId = 1,
                TimestampUtc = default,
                ContractAddress = "contract address",
                TransactionHash = "transaction hash",
                Address = "address"
            }).SpikePersistInt();
            
            
            var actual1 = userTicketContainer.GetAllTickets().Where(x => x.IsCheckedIn);
            Assert.Empty(actual1);
            
            var actual2 = userTicketContainer.GetAllTickets().Where(x => !x.IsCheckedIn);
            Assert.Single(actual2);
        }
    }
}

// purchase, transfer, ongoing, past
