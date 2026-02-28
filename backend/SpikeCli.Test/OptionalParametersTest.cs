namespace SpikeCli.Test;

public class OptionalParametersTest
{
    [Fact]
    public void Handle_two_args_and_two_options()
    {
        var cmdInfo = CreateTestCommandInfo();
        
        var args = """one to three "fo ur" -a stuff -b 42""".ToArgArray();
        
        var actual = new CliRunner(new List<CommandInfo>())
            .CmdPartsToActionParams(cmdInfo, args);
        
        var expected = new object[] { "three", "fo ur", "stuff", 42 };
        Assert.Equal(expected, actual.ToArray());
    }
    
    [Fact]
    public void Handle_two_args_and_one_option_last_option_is_empty()
    {
        var cmdInfo = CreateTestCommandInfo();
        
        var args = """one to three "fo ur" -a stuff""".ToArgArray();
        
        var actual = new CliRunner(new List<CommandInfo>())
            .CmdPartsToActionParams(cmdInfo, args);
        
        var expected = new object?[] { "three", "fo ur", "stuff", null };
        Assert.Equal(expected, actual.ToArray());
    }
    
    [Fact]
    public void Handle_two_args_and_one_option_first_option_is_empty()
    {
        var cmdInfo = CreateTestCommandInfo();
        
        var args = """one to three "fo ur" -b 42""".ToArgArray();
        
        var actual = new CliRunner(new List<CommandInfo>())
            .CmdPartsToActionParams(cmdInfo, args);
        
        var expected = new object?[] { "three", "fo ur", null, 42 };
        Assert.Equal(expected, actual.ToArray());
    }
    
    [Fact]
    public void Handle_two_args_and_no_option_Two_options_are_empty()
    {
        var cmdInfo = CreateTestCommandInfo();
        
        var args = """one to three "fo ur" """.ToArgArray();
        
        var actual = new CliRunner(new List<CommandInfo>())
            .CmdPartsToActionParams(cmdInfo, args);
        
        var expected = new object?[] { "three", "fo ur", null, null };
        Assert.Equal(expected, actual.ToArray());
    }

    private static CommandInfo CreateTestCommandInfo()
    {
        var cmdInfo = new CommandInfo("one", "two", 4);
        cmdInfo.AddParam<string>(name: "A1");
        cmdInfo.AddParam<string>(name: "A2");
        cmdInfo.AddOption<string>("-a");
        cmdInfo.AddOption<int>("-b");
        return cmdInfo;
    }
}


// Option at the end without a value - e.g., one to three four -a throws an exception, but no test verifies this behavior

// Option value is another option flag - e.g., -a -b where -b is incorrectly treated as the value for -a

// Duplicate options - e.g., -a foo -a bar only uses the first occurrence

// Option appears before arguments - e.g., one to -a stuff three four would fail since arguments are read by position
//     Fewer arguments than expected - e.g., one to three with only 1 argument when 2 are expected causes index out of bounds
// Empty command input - no verb/noun provided
//     Option value contains the option prefix - e.g., -a "-b" where the value literally starts with -
//     Boolean flag options (noted in your todo) - options like -v that don't take a value
// optionsStartIndex is unused - declared but never used, suggesting incomplete validation that options appear after arguments
//     Argument value matches an option name - e.g., one to -a four -a stuff where -a is both an argument value and an option