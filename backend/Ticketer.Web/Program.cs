using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;

using Ticketer.Model;
using Ticketer.Repository;
using Ticketer.UseCases;
using Ticketer.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging => logging.AddConsole());
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRepository();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Events", "");
});

builder.Services.AddSingleton<IJobQueue>(_ => new JobQueue(100));
builder.Services.AddHostedService<WorkerService>();
builder.Services.AddRepository();

builder.Services.AddAllUseCases();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


// todo map /organizer/usher   contractId ticketId userId? secret 
MapPostProcessUsherCheckIn(app);

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

// SpikeDbConfig.GetInstance().SetRootFolder("/Users/mikkel/ticketer");

app.Run();


void MapPostProcessUsherCheckIn(WebApplication webApplication)
{
    webApplication.MapPost("/organizer/event/usher-ticket", async (
        HttpContext context, 
        UsherTicketDtoV1 dto,
        [FromServices] IDynamoDBContext dynamo) =>
    {
        try
        {
            // TODO FAKE AUTH - FIX UP
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token)) return Results.Unauthorized();
                authHeader = token;
            }

            // if (!int.TryParse(authHeader, out var fakeTokeIsReallyUserId))
            //     return Results.BadRequest();
            
            var maybeUserState = await dynamo.LoadAsync<UserState>(authHeader);

            if (maybeUserState is not { } usherUserState)
                return Results.Unauthorized();

            var usherTicketHandler = context.RequestServices.GetRequiredService<UsherTicketHandler>();
            await usherTicketHandler.Execute(new User(usherUserState), dto.ContractAddress, dto.TicketId, dto.CheckInSecret);

            return Results.Ok("EVENT_ENTERED");
        }
        catch (DomainInvariant e)
        {
            return Results.BadRequest(e.Message);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Results.InternalServerError(e.Message);
        }
    });
}