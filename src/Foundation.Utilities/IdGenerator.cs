namespace Naos.Foundation
{
    using System;
    using System.Threading;

    /// <summary>
    /// Inspired by <see href="https://github.com/aspnet/KestrelHttpServer/blob/6fde01a825cffc09998d3f8a49464f7fbe40f9c4/src/Kestrel.Core/Internal/Infrastructure/CorrelationIdGenerator.cs"/>,
    /// this class generates an efficient 20-bytes ID which is the concatenation of a <c>base36</c> encoded
    /// machine name and <c>base32</c> encoded <see cref="long"/> using the alphabet <c>0-9</c> and <c>A-V</c>.
    /// </summary>
    public sealed class IdGenerator
    {
        // origin: http://www.nimaara.com/2018/10/10/generating-ids-in-csharp/
        private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUV";

        private static readonly char[] Prefix = new char[6];

        private static readonly ThreadLocal<char[]> CharBufferThreadLocal =
            new ThreadLocal<char[]>(() =>
            {
                var buffer = new char[20];
                buffer[0] = Prefix[0];
                buffer[1] = Prefix[1];
                buffer[2] = Prefix[2];
                buffer[3] = Prefix[3];
                buffer[4] = Prefix[4];
                buffer[5] = Prefix[5];
                buffer[6] = '0';
                //buffer[6] = '-';
                return buffer;
            });

        private static long lastId = DateTime.UtcNow.Ticks;

        static IdGenerator() => PopulatePrefix();

        private IdGenerator()
        {
        }

        /// <summary>
        /// Returns a single instance of the <see cref="IdGenerator"/>.
        /// </summary>
        public static IdGenerator Instance { get; } = new IdGenerator();

        /// <summary>
        /// Returns an Id like: <c>XOGLN100HLHI1F5INOFA</c>.
        /// </summary>
        public string Next => Generate(Interlocked.Increment(ref lastId));

        private static string Generate(long id)
        {
            var buffer = CharBufferThreadLocal.Value;

            buffer[7] = Characters[(int)(id >> 60) & 31];
            buffer[8] = Characters[(int)(id >> 55) & 31];
            buffer[9] = Characters[(int)(id >> 50) & 31];
            buffer[10] = Characters[(int)(id >> 45) & 31];
            buffer[11] = Characters[(int)(id >> 40) & 31];
            buffer[12] = Characters[(int)(id >> 35) & 31];
            buffer[13] = Characters[(int)(id >> 30) & 31];
            buffer[14] = Characters[(int)(id >> 25) & 31];
            buffer[15] = Characters[(int)(id >> 20) & 31];
            buffer[16] = Characters[(int)(id >> 15) & 31];
            buffer[17] = Characters[(int)(id >> 10) & 31];
            buffer[18] = Characters[(int)(id >> 5) & 31];
            buffer[19] = Characters[(int)id & 31];

            return new string(buffer, 0, buffer.Length);
        }

        private static void PopulatePrefix()
        {
            var machine = Base36.Encode(
                Math.Abs(Environment.MachineName.GetHashCode()));

            var i = Prefix.Length - 1;
            var j = 0;
            while (i >= 0)
            {
                if (j < machine.Length)
                {
                    Prefix[i] = machine[j];
                    j++;
                }
                else
                {
                    Prefix[i] = '0';
                }

                i--;
            }
        }
    }
}