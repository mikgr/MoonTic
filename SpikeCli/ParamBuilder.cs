namespace SpikeCli;

public class ParamBuilder(CliBuilder cliBuilder, CommandInfo cmdInf)
{
    public ParamBuilder Arg<TP1>(string p1Name)
    {
        cmdInf.AddParam<TP1>(p1Name);
        return this;
    }
    
    public ParamBuilder Opt<TO>(string optionName)
    {
        cmdInf.AddOption<TO>(optionName);
        return this;
    }
     

    public CliBuilder Enter()
    {
        cmdInf.Validate();
        return cliBuilder;
    }
}