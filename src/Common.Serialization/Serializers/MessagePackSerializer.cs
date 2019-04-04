namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using MessagePack.Resolvers;

    public class MessagePackSerializer : ISerializer
    {
        private readonly MessagePack.IFormatterResolver formatterResolver;
        private readonly bool useCompression;

        public MessagePackSerializer(MessagePack.IFormatterResolver resolver = null, bool useCompression = false)
        {
            this.useCompression = useCompression;
            this.formatterResolver = resolver ?? ContractlessStandardResolver.Instance;
        }

        public void Serialize(object value, Stream output)
        {
            if(this.useCompression)
            {
                MessagePack.LZ4MessagePackSerializer.NonGeneric.Serialize(value.GetType(), output, value, this.formatterResolver);
            }
            else
            {
                MessagePack.MessagePackSerializer.NonGeneric.Serialize(value.GetType(), output, value, this.formatterResolver);
            }
        }

        public object Deserialize(Stream input, Type type)
        {
            if(this.useCompression)
            {
                return MessagePack.LZ4MessagePackSerializer.NonGeneric.Deserialize(type, input, this.formatterResolver);
            }
            else
            {
                return MessagePack.MessagePackSerializer.NonGeneric.Deserialize(type, input, this.formatterResolver);
            }
        }

        public T Deserialize<T>(Stream input)
        {
            if(this.useCompression)
            {
                return MessagePack.LZ4MessagePackSerializer.Deserialize<T>(input, this.formatterResolver);
            }
            else
            {
                return MessagePack.MessagePackSerializer.Deserialize<T>(input, this.formatterResolver);
            }
        }
    }
}
