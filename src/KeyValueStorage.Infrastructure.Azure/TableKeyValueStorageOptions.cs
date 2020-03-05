namespace Naos.KeyValueStorage.Infrastructure
{
    using Naos.Foundation;

    public class TableKeyValueStorageOptions : OptionsBase
    {
        public string ConnectionString { get; set; } // dpending on connectionstring cosmosdb tables or storage tables will be used

        public int MaxInsertLimit { get; set; } = 100;
    }
}
