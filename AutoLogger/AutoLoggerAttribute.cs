using System;

namespace TYTCapstone
{
    public enum LoggerVerbosity {
        Verbose,
        Minimal,
        None,
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class AutoLogAttribute : Attribute
    {
        public static LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Verbose;

        public AutoLogAttribute() { }

        public AutoLogAttribute(LoggerVerbosity verbosity) {
            Verbosity = verbosity;
        }  
    }
}
