using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Reflection;
using TYTCapstone;

public static class Logger
{
    public static void LogPublicFields(object obj, LoggerVerbosity verbosity)
    {
        if (obj == null)
        {
            Console.WriteLine("<< AUTOLOG >> Object is null.");
            return;
        }

        if (verbosity is LoggerVerbosity.None)
        {
            return;
        }

        Console.WriteLine($"\n\n<< AUTOLOG >> Visiting: {obj.GetType().Name}");

        if (obj is ParserRuleContext context)
        {
            Console.WriteLine($"<< AUTOLOG >> Context Text: {context.GetText()}");

            if (verbosity is LoggerVerbosity.Minimal)
            {
                return;
            }

            Console.WriteLine("    Children:");
            foreach (IParseTree child in context.children)
            {
                Console.WriteLine($"        {child.ToString()} | {child.Payload}");
            }

            Console.WriteLine("\n    Contexts:");
            foreach (MethodInfo cntx in context.GetType().GetMethods())
            {
                if (cntx.ReturnType.GetInterfaces().Contains(typeof(IRuleNode)) || cntx.ReturnType.IsArray)
                {
                    Console.WriteLine("        " + cntx.Name);
                }
            }

            Console.WriteLine("\n    Terminal Nodes:");
            foreach (MethodInfo token in context.GetType().GetMethods())
            {
                if (token.ReturnType == typeof(ITerminalNode))
                {
                    Console.WriteLine("        " + token.Name);
                }
            }
        }
        else
        {
            Console.WriteLine($"<< AUTOLOG >> Error getting context, context is not ParserRuleContext.\n\n");
        }

    }
}