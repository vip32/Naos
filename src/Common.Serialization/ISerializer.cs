namespace Naos.Core.Common
{
    using System;
    using System.IO;

    public interface ISerializer
    {
        void Serialize(object value, Stream output);

        object Deserialize(Stream input, Type type);

        T Deserialize<T>(Stream input);
    }
}
