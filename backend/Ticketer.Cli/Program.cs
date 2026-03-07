﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpikeCli;
using Ticketer.Model;
using Ticketer.Repository;
using Ticketer.UseCases;

namespace Ticketer;

public static class Program
{
    public static void Main(string[] args)
    {
        User? currentUser = null;

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        var services = new ServiceCollection()
            .AddAllUseCases()
            .AddRepository(config)
            .BuildServiceProvider();
        
        var runner = new CliBuilder(services)
            // user crud
            .CmdDi("new", "user", "user-name", "email", (IServiceProvider s, string userName, string email) => 
                s.GetRequiredService<CreateUserHandler>().Execute(userName, email).Wait())
            
            .CmdDi("set", "user", "user-id", (IServiceProvider s, string userId) =>
            {
                HandleSetUser(userId, s);
            })
            
            .Cmd("create", "table", ()=> new SetUpDynamoTables().Execute(services.GetRequiredService<IOptions<DynamoDbSettings>>()))
            
                
            .CmdDi("print", "secret", "contract-id", "ticket-id", (IServiceProvider s, string contractId, int ticketId) => 
                s.GetRequiredService<PrintSecretHandler>().Execute(currentUser, contractId, ticketId))
            
            // event crud
            .CmdDi("new", "event", "event-name", "venue-open-time", "venue-close-time", "venue-time-zone", "ticket-count", "price",
                (IServiceProvider s, string name, DateTime venueOpenTime, DateTime venueCloseTime, string venueTimeZone, int tickets, decimal price) => 
                    s.GetRequiredService<NewEventInfoHandler>().Execute(currentUser!, name, "full venue addresss" ,venueOpenTime, venueCloseTime, venueTimeZone, tickets, price).Wait())
            
            .CmdDi("publish", "event", "event-info-id", (IServiceProvider s, string eventInfoId) => 
                s.GetRequiredService<PublishEventHandler>().Execute(eventInfoId, currentUser!).Wait())
            
            .CmdDi("buy", "ticket", "event-contract-id", (IServiceProvider s, string eventContractId) => 
                s.GetRequiredService<BuyTicketHandler>().Execute(eventContractId, currentUser!).Wait())
            
            .CmdDi("transfer", "ticket", "event-id", "ticket-id", "to-address", (IServiceProvider s, string eventId, int ticketId, string toAddress) => 
                s.GetRequiredService<TransferTicketHandler>().Execute(currentUser!, eventId, ticketId, toAddress).Wait())
            
            // new ask
            // cancel ask
            // accept ask
            
            .CmdDi("checkin", "ticket", "event-id", "ticket-id", (IServiceProvider s, string eventId, int ticketId) => 
                s.GetRequiredService<CheckInTicketHandler>().Execute(currentUser!, eventId, ticketId).Wait())
            
            .CmdDi("checkout", "ticket", "event-id", "ticket-id", (IServiceProvider s, string eventId, int ticketId) => 
                s.GetRequiredService<CheckOutTicketHandler>().Execute(currentUser, eventId, ticketId).Wait())
            
            // .Cmd("proof", "checkin", "event-id", "ticket-id", "secret", (int eventId, int ticketId, string secret) => 
            //     HandleProofCheckin(eventId, ticketId, secret))
            
            .CmdDi("enter", "event", "contract-address", "ticket-id", "secret", 
                (IServiceProvider s, string contractAddress, int ticketId, string secret) => 
                s.GetRequiredService<UsherTicketHandler>()
                    .Execute(usherUser: currentUser!, contractAddress, ticketId, secret).Wait())
            
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

    // private static void HandleProofCheckin(int eventId, int ticketId, string secret)
    // {
    //     var contract = SpikeRepo.ReadIntId<EventContract>(eventId);
    //     if (contract.ProofByTicketHolder(ticketId, secret))
    //         Console.WriteLine("Proof successful");
    //     else
    //         Console.WriteLine("Proof failed");
    // }

    
    private static User? HandleSetUser(string userId, IServiceProvider services)
    {
        var repo = services.GetRequiredService<IRepository>();
        var user = repo.LoadUserAsync(userId);
        user.Wait();
        
        return user.Result;
    }
}