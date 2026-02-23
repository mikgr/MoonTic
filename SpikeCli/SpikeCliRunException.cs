namespace SpikeCli;

public class SpikeCliRunException : Exception
{
    public SpikeCliRunException() { }

    public SpikeCliRunException(string message) : base(message) { }

    public SpikeCliRunException(string message, Exception innerException)
        : base(message, innerException) { }
}