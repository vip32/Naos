namespace Naos.Foundation
{
    using System.Threading.Tasks;

    public interface INodeRenderOptions
    {
        string RootNodeBreak { get; set; }

        string ChildNodeBreak { get; set; }

        string Corner { get; set; }

        string Cross { get; set; }

        string Space { get; set; }

        string Vertical { get; set; }

        Task WriteAsync(string value);
    }
}