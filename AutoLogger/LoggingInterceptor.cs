using Castle.DynamicProxy;
using System;
using System.Linq;
using TYTCapstone;

public class LoggingInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // Check if the method has the LogPublicFields attribute
        var hasLoggingAttribute = invocation.Method.GetCustomAttributes(typeof(AutoLogAttribute), true).Any();

        if (hasLoggingAttribute)
        {
            // Log public fields of each parameter
            foreach (var arg in invocation.Arguments)
            {
                Logger.LogPublicFields(arg, AutoLogAttribute.Verbosity);
            }
        }

        // Proceed with the original method call
        invocation.Proceed();
    }
}