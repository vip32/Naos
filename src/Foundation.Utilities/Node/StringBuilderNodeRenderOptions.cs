namespace Naos.Foundation
{
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;

    public class StringBuilderNodeRenderOptions : INodeRenderOptions
    {
        private readonly StringBuilder builder;

        public StringBuilderNodeRenderOptions(StringBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            this.builder = builder;
        }

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
                this.builder.Append(value);
            }

            return Task.CompletedTask;
        }
    }
}
