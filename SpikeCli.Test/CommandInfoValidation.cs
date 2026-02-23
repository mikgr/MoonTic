namespace SpikeCli.Test;

public class CommandInfoValidation
{
    [Fact]
    public void Validate_no_duplicate_param_names()
    {
        var inf = new CommandInfo ( "do", "stuff", 2);
        inf.AddParam<string>("a");
        inf.AddParam<string>("a");
        
        Assert.Throws<SpikeCliRunException>(() => inf.Validate());
    }
    
    [Fact]
    public void Validate_no_duplicate_option_names()
    {
        var inf = new CommandInfo( "do", "stuff",  2);
        inf.AddOption<string>("a");
        inf.AddOption<string>("a");
        
        Assert.Throws<SpikeCliRunException>(() => inf.Validate());
    }
}