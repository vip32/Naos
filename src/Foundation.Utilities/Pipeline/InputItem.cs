namespace Naos.Foundation
{
    using System.Threading.Tasks;

    public partial class Pipeline<TPipeIn, TPipeOut>
    {
        public class InputItem<T>
        {
            public T Value { get; set; }

            public TaskCompletionSource<TPipeOut> TaskCompletionSource { get; set; }
        }
    }
}
