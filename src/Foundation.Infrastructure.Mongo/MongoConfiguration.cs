namespace Naos.Foundation.Infrastructure
{
    public class MongoConfiguration
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017?connectTimeoutMS=300000";

        public string DatabaseName { get; set; } = "master";

        //public string ApplicationName { get; set; }

        public bool LoggingEnabled { get; set; } = true;
    }
}
