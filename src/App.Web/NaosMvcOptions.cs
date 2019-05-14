namespace Naos.Core.App.Web
{
    using System;
    using System.Collections.Generic;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Newtonsoft.Json;

    public class NaosMvcOptions
    {
        private readonly IList<GenericRepositoryControllerInformation> controllerRegistrations = new List<GenericRepositoryControllerInformation>();

        public IEnumerable<GenericRepositoryControllerInformation> ControllerRegistrations
        {
            get
            {
                return this.controllerRegistrations;
            }
        }

        public JsonSerializerSettings JsonSerializerSettings { get; private set; }

        public NaosMvcOptions AddJsonSerializerSettings(JsonSerializerSettings settings)
        {
            this.JsonSerializerSettings = settings;

            return this;
        }

        public NaosMvcOptions AddRepositoryController(Type entityType, Type repositoryType = null)
        {
            this.controllerRegistrations.Add(new GenericRepositoryControllerInformation
            {
                EntityType = entityType,
                RepositoryType = repositoryType
            });

            return this;
        }

        public NaosMvcOptions AddRepositoryController<TEntity>()
            where TEntity : class, IEntity, IAggregateRoot
        {
            this.controllerRegistrations.Add(new GenericRepositoryControllerInformation
            {
                EntityType = typeof(TEntity)
            });

            return this;
        }

        public NaosMvcOptions AddRepositoryController<TEntity, TRepo>()
            where TEntity : class, IEntity, IAggregateRoot
            where TRepo : class, IGenericRepository<TEntity>
        {
            this.controllerRegistrations.Add(new GenericRepositoryControllerInformation
            {
                EntityType = typeof(TEntity),
                RepositoryType = typeof(TRepo)
            });

            return this;
        }
    }
}
