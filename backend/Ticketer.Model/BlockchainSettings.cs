using System.ComponentModel.DataAnnotations;

namespace Ticketer.Model;

public class BlockchainSettings
{
    [Required]
    [Url]
    public required string BlockchainRpcUrl { get; init; }
    
    [Required]
    [Range(999,9999999)]
    public required int BlockchainId { get; init; }
    
    // todo dont use config of private key for prod, sign in AWS KMS
    [Required]
    [MinLength(64), MaxLength(64)]
    public required string SystemPrivateKey { get; init; }
}