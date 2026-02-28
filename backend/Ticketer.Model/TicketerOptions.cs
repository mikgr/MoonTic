using System.Numerics;

namespace Ticketer.Model;

public static class TicketerOptions
{
    public static string BlockchainRpcUrl = "http://127.0.0.1:9650/ext/bc/C/rpc";
    public static BigInteger BlockchainId = 1337;
    public static string PrivateKey = "PK";
}