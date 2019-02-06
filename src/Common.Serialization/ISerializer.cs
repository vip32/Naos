namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;

    public interface ISerializer
    {
        object Deserialize(Stream data, Type objectType);

        void Serialize(object value, Stream output);
    }
}
