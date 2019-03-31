namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;

    public interface ISerializer
    {
        void Serialize(object value, Stream output);

        object Deserialize(Stream input, Type type);
    }
}
