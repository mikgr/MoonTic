namespace SpikeCli.Test;

public class ArgStringToArgArrayTest
{
    [Fact]
    public void ToArgArray_can_handle_double_quotes()
    {
        var argsString = """aa bb "hello world" dd""";

        var partList = argsString.ToArgArray();

        string[] expected = ["aa", "bb", "hello world", "dd"]; 
        string[] actual = partList.ToArray();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ToArgArray_will_clear_extra_spaces()
    {
        var argsString = """aa  bb "hello world"   dd""";

        var partList = argsString.ToArgArray();

        string[] expected = ["aa", "bb", "hello world", "dd"]; 
        string[] actual = partList.ToArray();
        Assert.Equal(expected, actual);
    }
    
}