namespace SpikeCli;

public record ParamInfo
{
    public required Type Type {get; init;}
    public required string Name { get; init; }
}