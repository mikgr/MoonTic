using System.Globalization;
using System.Reflection;

namespace SpikeCli;

public class CliRunner
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly Dictionary<(string verb,string noun), CommandInfo> _commands;
    
    internal CliRunner(List<CommandInfo> commands, IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        _commands = commands.ToDictionary(c => c.GetKey(), c => c);
    }
    
    public void Run(string[] args)
    {
        try
        {
            var verb = args[0];

            switch (verb)
            {
                case "help":
                    PrintHelp();
                    return;

                case "clear":
                    Console.Clear();
                    return;
            }

            if (args.Length == 1)
            {
                Console.Error.WriteLine($"missing command. Input {verb}");
                return;
            }

            var noun = args[1];

            if (FindCommand(verb, noun) is not { } cmd)
            {
                Console.Error.WriteLine("unknown command");
                return;
            }

            var paramObjects = CmdPartsToActionParams(cmd, args);

            var expectedParameterCount = cmd.WithDependencyInjection
                ? cmd.ParamCount + 1
                : cmd.ParamCount;
            
            if (paramObjects.Count != expectedParameterCount)
                throw new SpikeCliRunException("Wrong number of parameters");
            
            if (cmd.Action is { } commandAction)
                commandAction(paramObjects.ToArray());
            else
                throw new SpikeCliRunException("no action defined for command");
        }
        catch (SpikeCliRunException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            if (_exceptionHandler is {} h) 
                h.Invoke(e);
            else 
                Console.WriteLine(e);
        }
    }

    Action<Exception>? _exceptionHandler;
    public CliRunner SetExceptionHandler(Action<Exception> handler)
    {
        _exceptionHandler = handler;
        return this;
    }

    private CommandInfo? FindCommand(string verb, string noun)
    {
        if (_commands.TryGetValue((verb, noun), out var command))
            return command;
        
        PrintHelp();

        return null;
    }
    
    internal List<object?> CmdPartsToActionParams(CommandInfo cmdInfo, string[] cmdParts)
    {
        var parameters = new List<object?>();

        if (cmdInfo.WithDependencyInjection)
        {
            if (_serviceProvider is not {} sp) 
                throw new SpikeCliRunException($"ServiceProvider not set. Cmd {cmdInfo.GetKey()} requires DI");
            
            parameters.Add(sp);
        }

        const int nounVerbOffset = 2;
        
        // add arguments to param-list
        for (var i = nounVerbOffset; i < cmdInfo.ArgsCount + nounVerbOffset; i++)
        {
            var paramInfo = cmdInfo.GetArgAt(index: i - nounVerbOffset);
            if (cmdParts.Length == i) 
                throw new SpikeCliRunException($"Missing parameter: '{paramInfo.Name}'");
                 
            // todo handle . , conversions of string to decimal: ifparamInfo.Type == typeof(decimal) THEN (object)Convert.ToDecimal(cmdParts[i], invariantCulture)
            // todo add tests and config around culture info
            var valueObj = paramInfo.Type == typeof(decimal)
                ? Convert.ToDecimal(cmdParts[i], CultureInfo.InvariantCulture)
                : Convert.ChangeType(cmdParts[i], paramInfo.Type);
            
            parameters.Add(valueObj);
        }
        
        // add options or null to param-list
        foreach (var opt in cmdInfo.Options)
        {
            var optionIndex = cmdParts.IndexOf(opt.Name);
            if (optionIndex == -1)
            {
                parameters.Add(null);
                continue;
            }
            
            var valuePos = optionIndex + 1;
            if (valuePos >= cmdParts.Length) 
                throw new SpikeCliRunException($"Missing value for option {opt.Name}");

            var stringVal = cmdParts[valuePos];    
            var paramObject = Convert.ChangeType(stringVal, opt.Type);
            parameters.Add(paramObject);
        }

        return parameters;
    }

    private void PrintHelp()
    {
        Console.WriteLine("");
        foreach (var cmd in _commands.Values) 
            Console.Write(cmd.GetHelpText());
    }
}