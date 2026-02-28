using System.Numerics;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Util;
using SpikeDb;
using SpikeCli;
using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer;

public static class Program
{
    public static void Main(string[] args)
    {
        SpikeDbConfig.GetInstance().SetRootFolder("/Users/mikkel/ticketer");
        
        User? currentUser = null;

        var services = new ServiceCollection()
            .AddSingleton<PrintEventHandler>()
            .AddAllUseCases()
            .BuildServiceProvider();
        
        var runner = new CliBuilder(services)
            // user crud
            .CmdDi("new", "user", "user-name", "email", (IServiceProvider s, string userName, string email) => 
                s.GetRequiredService<CreateUserHandler>().Execute(userName, email))
            
            .Cmd("ls", "user", HandleListUsers)
            .Cmd("set", "user", "user-id", (int id) => currentUser = HandleSetUser(id))
                
            .CmdDi("print", "secret", "contract-id", "ticket-id", (IServiceProvider s, int contractId, int ticketId) => 
                s.GetRequiredService<PrintSecretHandler>().Execute(currentUser, contractId, ticketId))
            
            // event crud
            .Cmd("new", "event", "event-name", "venue-open-time", "venue-close-time", "ticket-count", "price",
                (string name, DateTime venueOpenTime, DateTime venueCloseTime, int tickets, decimal price) => 
                    NewEventInfoHandler.Execute(currentUser, name, venueOpenTime, venueCloseTime, tickets, price))
            
            .Cmd("ls", "event-info", () => HandleLsEventInfo(currentUser))
            .Cmd("rm", "event-info", "id", (int eventInfoId) => HandleRmEventInfo(eventInfoId, currentUser))
            // todo must deploy 
            .CmdDi("publish", "event", "event-info-id", (IServiceProvider s, int eventInfoId) => 
                s.GetRequiredService<PublishEventHandler>().Execute(eventInfoId, currentUser).Wait())
            
            // contract
            .CmdDi("print", "event", "id", (IServiceProvider s, int eventId) => 
                s.GetRequiredService<PrintEventHandler>().Execute(eventId))
            
            .CmdDi("buy", "ticket", "event-contract-id", (IServiceProvider s, int eventContractId) => 
                s.GetRequiredService<BuyTicketHandler>().Execute(eventContractId, currentUser).Wait())
            
            .Cmd("ls", "event", HandleLsEvent)
            
            .CmdDi("transfer", "ticket", "event-id", "ticket-id", "to-address", (IServiceProvider s, int eventId, int ticketId, string toAddress) => 
                s.GetRequiredService<TransferTicketHandler>().Execute(currentUser, eventId, ticketId, toAddress).Wait())
            
            // new ask
            // cancel ask
            // accept ask
            
            .CmdDi("checkin", "ticket", "event-id", "ticket-id", (IServiceProvider s, int eventId, int ticketId) => 
                s.GetRequiredService<CheckInTicketHandler>().Execute(currentUser, eventId, ticketId).Wait())
            
            .CmdDi("checkout", "ticket", "event-id", "ticket-id", (IServiceProvider s, int eventId, int ticketId) => 
                s.GetRequiredService<CheckOutTicketHandler>().Execute(currentUser, eventId, ticketId).Wait())
            
            .Cmd("proof", "checkin", "event-id", "ticket-id", "secret", (int eventId, int ticketId, string secret) => 
                HandleProofCheckin(eventId, ticketId, secret))
            
            .CmdDi("enter", "event", "contract-address", "ticket-id", "secret", 
                (IServiceProvider s, string contractAddress, int ticketId, string secret) => 
                s.GetRequiredService<UsherTicketHandler>().Execute(usherUser: currentUser, contractAddress, ticketId, secret))
            
            .CmdDi("deploy", "contract", "event-id", (IServiceProvider s, int eventInfoId) =>
            {
                // todo merge with publish
                
                var eventContract = SpikeRepo.ReadOrNullByInt<EventContract>(eventInfoId)
                    ?? throw new Exception("Event not found");
                
                // Constructor arguments
                BigInteger fakeCheckOutBlockedTime = eventContract.GetCheckOutBlockStart().ToUnixTimestamp();
                BigInteger venueOpenTime = eventContract.VenueOpenTime.ToUnixTimestamp();
                BigInteger venueCloseTime = eventContract.VenueCloseTime.ToUnixTimestamp();
                BigInteger totalTicketCount = eventContract.TotalTickets; // uint64 can be BigInteger in Nethereum
                string location = "Store VEGA, Enghavevej 40, 1674 Copenhagen V, Denmark"; // todo fix

                var constructorArgs = new object[]
                {
                    fakeCheckOutBlockedTime,
                    venueOpenTime,
                    venueCloseTime,
                    totalTicketCount,
                    location
                };
                
                Task.Run(async () => { await s.GetRequiredService<DeployContractHandler>().Execute(constructorArgs, eventContract); }).Wait();
            })
            // Crypto
            .CmdDi("mint", "ticket", "contract-address", "to-address", 
                (IServiceProvider s, string contractAddress, string toAddress) =>
                    s.GetRequiredService<MintTicketHandler>()
                        .Execute(contractAddress, toAddress).Wait())
            
            .Build();
    
        
        if (args.Length > 0)
        {
            var argString = string.Join('¤', args);
            var cmdStrings = argString.Split('+')
                .Where(c => !string.IsNullOrWhiteSpace(c));
            
            foreach (var cmdString in cmdStrings)
            {
                var cmdArgs = cmdString.Split('¤')
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToArray();
                
                runner.Run(cmdArgs);
            }
            return;
        };
        
        
        while (true)
        {
            Console.Write($"{currentUser?.UserName??""}> ");

            try
            {
                // todo splitting by ' ' wil not work - we need to support "some more strings"
                var input = Console.ReadLine()?.ToArgArray() ?? [];

                if (input[0] == "exit") break;

                runner.Run(input);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private static void HandleProofCheckin(int eventId, int ticketId, string secret)
    {
        var contract = SpikeRepo.ReadIntId<EventContract>(eventId);
        if (contract.ProofByTicketHolder(ticketId, secret))
            Console.WriteLine("Proof successful");
        else
            Console.WriteLine("Proof failed");
    }

    private static void HandleLsEvent()
    {
        var events = SpikeRepo.ReadCollection<EventContract>();
        Console.WriteLine(JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void HandlePrintEvent(int eventId)
    {
        var eventContract = SpikeRepo.ReadIntId<EventContract>(eventId);
        Console.WriteLine(JsonSerializer.Serialize(eventContract, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void HandleRmEventInfo(int eventInfoId, User? currentUser)
    {
        var eventInfo = SpikeRepo.ReadOrNullByInt<EventInfo>(eventInfoId);
        if (eventInfo is null) throw new Exception("Event not found");
        if (eventInfo.Owner != currentUser?.Id) throw new Exception("Not authorized");
                
        SpikeRepo.Delete<EventInfo>(eventInfoId);
    }

    private static void HandleLsEventInfo(User? currentUser)
    {
        if (currentUser is null) throw new Exception("User not set");

        SpikeRepo.ReadCollection<EventInfo>(x => x.Owner == currentUser.Id)
            .ToList()
            .ForEach(e => Console.WriteLine($"{e.Id} {e.Name} Start:{e.VenueOpenTime} End:{e.VenueCloseTime} {e.Tickets} {e.Price}"));
    }

    private static User? HandleSetUser(int userId)
    {
        var currentUser = SpikeRepo.ReadOrNullByInt<User>(userId)
            ?? throw new ArgumentException("User not found");
        
        return currentUser;
    }

    private static void HandleListUsers()
    {
        foreach (var user in SpikeRepo.ReadCollection<User>()) 
            Console.WriteLine($"{user.Id} {user.UserName} {user.Email}");
    }
}

public class PrintEventHandler
{
    public void Execute(int eventId)
    {
        var eventContract = SpikeRepo.ReadIntId<EventContract>(eventId);
        Console.WriteLine(JsonSerializer.Serialize(eventContract, new JsonSerializerOptions { WriteIndented = true }));
    }
}