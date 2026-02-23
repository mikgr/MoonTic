using SpikeDb;
using Ticketer.Model;
using Ticketer.UseCases;
using Ticketer.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Events", "");
});
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

SpikeDbConfig.GetInstance().SetRootFolder("/Users/mikkel/ticketer");

app.Run();


void MapPostProcessUsherCheckIn(WebApplication webApplication)
{
    webApplication.MapPost("/organizer/event/usher-ticket", (HttpContext context, UsherTicketDtoV1 dto) =>
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

            if (!int.TryParse(authHeader, out var fakeTokeIsReallyUserId))
                return Results.BadRequest();

            if (SpikeRepo.ReadOrNullByInt<User>(fakeTokeIsReallyUserId) is not { } usherUser)
                return Results.Unauthorized();

            var usherTicketHandler = context.RequestServices.GetRequiredService<UsherTicketHandler>();
            usherTicketHandler.Execute(usherUser, dto.ContractId, dto.TicketId, dto.CheckInSecret);

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