namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;

    public class CsvSerializer : ITextSerializer
    {
        public void Serialize(object value, Stream output)
        {
            // TODO: https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/CsvSerializer.cs AGPL!
            throw new NotImplementedException();
        }

        public object Deserialize(Stream input, Type type)
        {
            // TODO: https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/CsvSerializer.cs AGPL!
            throw new NotImplementedException();
        }

        public T Deserialize<T>(Stream input)
        {
            // TODO: https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/CsvSerializer.cs AGPL!
            throw new NotImplementedException();
        }
    }
}
