namespace Naos.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Naos.App.Web.Controllers;
    using Naos.Foundation;

    public class GenericRepositoryControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IEnumerable<GenericRepositoryControllerInformation> informations;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepositoryControllerFeatureProvider"/> class.
        /// </summary>
        /// <param name="informations">The informations.</param>
        public GenericRepositoryControllerFeatureProvider(
            IEnumerable<GenericRepositoryControllerInformation> informations)
        {
            this.informations = informations;
        }

        /// <summary>
        /// Updates the <paramref name="feature" /> instance.
        /// </summary>
        /// <param name="parts">The list of <see cref="T:Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPart" /> instances in the application.</param>
        /// <param name="feature">The feature instance to populate.</param>
        /// <remarks>
        /// <see cref="T:Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPart" /> instances in <paramref name="parts" /> appear in the same ordered sequence they
        /// are stored in <see cref="P:Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager.ApplicationParts" />. This ordering may be used by the feature
        /// provider to make precedence decisions.
        /// </remarks>
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            // https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/

            foreach (var information in this.informations.Safe())
            {
                if (information.EntityType == null)
                {
                    continue;
                }

                if (information.RepositoryType != null)
                {
                    Type[] args = { information.EntityType, information.RepositoryType };
                    feature.Controllers.Add(typeof(NaosGenericRepositoryControllerBase<,>).MakeGenericType(args).GetTypeInfo());
                }
                else
                {
                    Type[] args = { information.EntityType };
                    feature.Controllers.Add(typeof(NaosEntityGenericRepositoryControllerBase<>).MakeGenericType(args).GetTypeInfo());
                }
            }
        }
    }
}
