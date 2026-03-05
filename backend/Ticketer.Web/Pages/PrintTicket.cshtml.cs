using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

using Ticketer.Model;

namespace Ticketer.Web.Pages;


public class PrintTicket : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ContractAddress { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int? TicketId { get; set; }
    
    public string Secret { get; set; } = "";
    public string QrCodeBase64 { get; set; } = "";

    public EventContract Contract => _contract!;
    private EventContract? _contract = null;
    
    public async Task<IActionResult> OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToPage("/LogIn");
        
        if (TicketId is null) return NotFound();
        if (ContractAddress is null) return NotFound();
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        _contract = await repo.LoadContractBy(contractAddress: ContractAddress);

        if (_contract is null) return NotFound();

        var user = await repo.LoadUserAsync(userId);
        Secret = user.GetSecret(_contract.Id, TicketId.Value) ?? "n/a";

        var qrValue = $"moontic://usherticket/{ContractAddress}/{TicketId}/{Secret}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrValue, QRCodeGenerator.ECCLevel.L);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(5);
        QrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";

        return Page();
    }
}