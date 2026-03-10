using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ticketer.Model;
using Ticketer.Repository;
using Ticketer.UseCases;
using Ticketer.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging(logging => logging.AddConsole());
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Events", "");
});

builder.Services.AddSingleton<IJobQueue>(_ => new JobQueue(100));
builder.Services.AddHostedService<WorkerService>();

builder.Services.AddRepository(builder.Configuration);
builder.Services.AddAllUseCases(builder.Configuration);


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

app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


// todo extract endpoints
void MapPostProcessUsherCheckIn(WebApplication webApplication)
{
    webApplication.MapPost("/organizer/event/usher-ticket", async (
        HttpContext context, 
        UsherTicketDtoV1 dto,
        [FromServices] IRepository repo) =>
    {
        try
        {
            string? fakeAuthToken = null;
            // TODO FAKE AUTH - FIX UP
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                fakeAuthToken = token;
            }
            
            if (string.IsNullOrEmpty(fakeAuthToken)) return Results.Unauthorized();
            
            
            var maybeUser = await repo.LoadUserAsync(fakeAuthToken);

            if (maybeUser is not { } usherUserState)
                return Results.Unauthorized();

            var usherTicketHandler = context.RequestServices.GetRequiredService<UsherTicketHandler>();
            await usherTicketHandler.Execute(maybeUser, dto.ContractAddress, dto.TicketId, dto.CheckInSecret);

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