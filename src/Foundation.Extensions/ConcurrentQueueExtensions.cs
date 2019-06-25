namespace Naos.Foundation
{
    using System.Collections.Concurrent;

    public static class ConcurrentQueueExtensions
    {
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while(queue.TryDequeue(out var _))
            {
            }
        }
    }
}
