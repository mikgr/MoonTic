namespace SpikeCli;


public class CliBuilder(IServiceProvider? serviceProvider = null)
{
    // todo support handler classes with method as commands and dependency injection
    private readonly List<CommandInfo> _commandInfos = [];
    
    public CliBuilder Cmd(string verb, string noun, Action action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount:0);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke();
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder Cmd<TP1>(string verb, string noun, string p1Name, Action<TP1> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 1)
            .AddParam<TP1>(p1Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder CmdDi<TP1>(string verb, string noun, string p1Name, Action<IServiceProvider, TP1> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 1, withDependencyInjection: true)
            .AddParam<TP1>(p1Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    
    public CliBuilder Cmd<TP1,TP2>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        Action<TP1, TP2> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 3)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder CmdDi<TP1, TP2>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        Action<IServiceProvider, TP1, TP2> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 2, withDependencyInjection: true)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    
    public CliBuilder Cmd<TP1, TP2, TP3>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        Action<TP1, TP2, TP3> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 3)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name);

        cmdDef.Action = (parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder CmdDi<TP1, TP2, TP3>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        Action<IServiceProvider, TP1, TP2, TP3> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 3, withDependencyInjection: true)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }

    
    public CliBuilder Cmd<TP1, TP2, TP3, TP4>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        string p4Name,
        Action<TP1, TP2, TP3, TP4> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 4)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name)
            .AddParam<TP4>(p4Name);

        cmdDef.Action = (parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder CmdDi<TP1, TP2, TP3, TP4>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        string p4Name,
        Action<IServiceProvider, TP1, TP2, TP3, TP4> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 4, withDependencyInjection: true)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name)
            .AddParam<TP4>(p4Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder Cmd<TP1, TP2, TP3, TP4, TP5>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        string p4Name,
        string p5Name,
        Action<TP1, TP2, TP3, TP4, TP5> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 5)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name)
            .AddParam<TP4>(p4Name)
            .AddParam<TP5>(p5Name);

        cmdDef.Action = (parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder CmdDi<TP1, TP2, TP3, TP4, TP5>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        string p4Name,
        string p5Name,
        Action<IServiceProvider, TP1, TP2, TP3, TP4, TP5> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 5, withDependencyInjection: true)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name)
            .AddParam<TP4>(p4Name)
            .AddParam<TP5>(p5Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    public CliBuilder CmdDi<TP1, TP2, TP3, TP4, TP5, TP6>(
        string verb,
        string noun,
        string p1Name,
        string p2Name,
        string p3Name,
        string p4Name,
        string p5Name,
        string p6Name,
        Action<IServiceProvider, TP1, TP2, TP3, TP4, TP5, TP6> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 6, withDependencyInjection: true)
            .AddParam<TP1>(p1Name)
            .AddParam<TP2>(p2Name)
            .AddParam<TP3>(p3Name)
            .AddParam<TP4>(p4Name)
            .AddParam<TP5>(p5Name)
            .AddParam<TP6>(p6Name);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return this;
    }
    
    
    public ParamBuilder BldCmd<TP1,TP2>(string verb, string noun, Action<TP1, TP2> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 2);

        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };

        _commandInfos.Add(cmdDef);
        return new ParamBuilder(this, cmdDef);
    }
    
    public ParamBuilder BldCmd<TP1,TP2,TP3>(string verb, string noun, Action<TP1, TP2, TP3> action)
    {
        var cmdDef = CreateCmdInfo(verb, noun, argCount: 3);
    
        cmdDef.Action = (object?[] parameters) =>
        {
            action.DynamicInvoke(parameters);
            return null;
        };
    
        _commandInfos.Add(cmdDef);
        return new ParamBuilder(this, cmdDef);
    }
    
    private CommandInfo CreateCmdInfo(
        string verb, 
        string noun, 
        uint argCount, 
        bool withDependencyInjection = false) => 
        new (verb, noun, argCount, withDependencyInjection);
    
    public CliRunner Build() => 
        new(_commandInfos, serviceProvider);
}
