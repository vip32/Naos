namespace Naos.Core.App.Web
{
    using System;
    using System.Collections.Generic;
    using Naos.Foundation.Domain;
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

        /// <summary>
        /// Adds the generic repository controller for the specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        public NaosMvcOptions AddGenericRepositoryController<TEntity>()
            where TEntity : class, IEntity, IAggregateRoot
        {
            this.controllerRegistrations.Add(new GenericRepositoryControllerInformation
            {
                EntityType = typeof(TEntity)
            });

            return this;
        }

        /// <summary>
        /// Adds the generic repository controller for the specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TRepository">The type of the repo.</typeparam>
        public NaosMvcOptions AddGenericRepositoryController<TEntity, TRepository>()
            where TEntity : class, IEntity, IAggregateRoot
            where TRepository : class, IGenericRepository<TEntity>
        {
            this.controllerRegistrations.Add(new GenericRepositoryControllerInformation
            {
                EntityType = typeof(TEntity),
                RepositoryType = typeof(TRepository)
            });

            return this;
        }
    }
}
