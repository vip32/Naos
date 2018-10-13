namespace Naos.Core.Common
{
    using System;

    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception source)
        {
            return source.InnerException == null
                 ? source.Message.Replace(Environment.NewLine, Environment.NewLine + " ")
                 : $"{source.Message}  --> {source.InnerException.GetFullMessage()}".Replace(Environment.NewLine, Environment.NewLine + " ");
        }
    }
}
