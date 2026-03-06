using System.Numerics;

namespace Ticketer.Model;

// todo real config
public static class TicketerOptions
{
    public static string BlockchainRpcUrl = "http://127.0.0.1:9650/ext/bc/C/rpc";
    public static BigInteger BlockchainId = 1337;
    public static string SystemPrivateKey = "56289e99c94b6912bfc12adc093c9b51124f0dc54ac7a766b2bc5ccf558d8027";
}