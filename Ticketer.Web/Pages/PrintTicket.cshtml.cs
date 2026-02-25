using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using SpikeDb;
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
    
    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null or -1) return RedirectToPage("/LogIn");
        
        if (TicketId is null) return NotFound();
        
        _contract = SpikeRepo.ReadFirstOrDefault<EventContract>(x =>
            x.ContractAddress.ToLower() == ContractAddress?.ToLower());

        if (_contract is null) return NotFound();
        
        var user = SpikeRepo.ReadSingle<User>(x => x.Id == userId);
        Secret = user.GetSecret(_contract.Id, TicketId.Value) ?? "n/a";

        var qrValue = $"moontic://usherticket/{ContractAddress}/{TicketId}/{Secret}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrValue, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(20);
        QrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";

        return Page();
    }
}