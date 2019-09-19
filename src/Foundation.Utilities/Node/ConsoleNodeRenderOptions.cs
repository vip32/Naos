namespace Naos.Foundation
{
    using System;
    using System.Threading.Tasks;

    public class ConsoleNodeRenderOptions : INodeRenderOptions
    {
        public string RootNodeBreak { get; set; } = "\n";

        public string ChildNodeBreak { get; set; } = "\n";

        public string Cross { get; set; } = " ├─"; // " ├─";

        public string Corner { get; set; } = " └─"; //" └─";

        public string Vertical { get; set; } = " │ "; //" │ ";

        public string Space { get; set; } = "   "; //"   ";

        public Task WriteAsync(string value)
        {
            if (!value.IsNullOrEmpty())
            {
                Console.Write(value);
            }

            return Task.CompletedTask;
        }
    }
}
