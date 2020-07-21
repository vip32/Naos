namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;

#pragma warning disable SA1402 // File may only contain a single class
    /// <summary>
    /// Creates instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type with a parameterless constructor.</typeparam>
    public static class Factory<T>
        where T : class
    {
        /// <summary>
        /// Create an instance by using compiled lambda expressions
        /// https://vagifabilov.wordpress.com/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/.
        /// </summary>
        private static readonly Func<T> CreateFunc =
            Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

        /// <summary>
        /// Creates an instance of type <typeparamref name="T"/> by calling it's parameterless constructor.
        /// </summary>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public static T Create() => CreateFunc(); // without ctor, fast

        /// <summary>
        /// Creates an instance of type <typeparamref name="T"/> by calling it's parameterless constructor.
        /// </summary>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public static T Create(IDictionary<string, object> propertyItems)
        {
            var instance = CreateFunc(); // without ctor, fast
            ReflectionHelper.SetProperties(instance, propertyItems);
            return instance;
        }

        //public static object Create(Type type) => Activator.CreateInstance(type);

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the constructor that best matches
        ///  the specified parameters.
        /// </summary>
        /// <param name="parameters"></param>
        public static T Create(params object[] parameters)
        {
            try
            {
                return Activator.CreateInstance(typeof(T), parameters) as T;
            }
            catch (MissingMethodException)
            {
                return default;
            }
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the constructor that best matches
        ///  the specified parameters.
        /// </summary>
        /// <param name="propertyItems"></param>
        /// <param name="parameters"></param>
        public static T Create(IDictionary<string, object> propertyItems, params object[] parameters)
        {
            try
            {
                var instance = Activator.CreateInstance(typeof(T), parameters) as T;
                ReflectionHelper.SetProperties(instance, propertyItems);
                return instance;
            }
            catch (MissingMethodException)
            {
                return default;
            }
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the serviceprovider to
        ///  get instances for the constructor.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static T Create(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            return ActivatorUtilities.CreateInstance<T>(serviceProvider);
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the serviceprovider to
        ///  get instances for the constructor.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static T Create(IServiceProvider serviceProvider, params object[] parameters)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            return ActivatorUtilities.CreateInstance<T>(serviceProvider, parameters);
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the serviceprovider to
        ///  get instances for the constructor.
        /// </summary>
        /// <param name="propertyItems"></param>
        /// <param name="serviceProvider"></param>
        public static T Create(IDictionary<string, object> propertyItems, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider);
            ReflectionHelper.SetProperties(instance, propertyItems);
            return instance;
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the serviceprovider to
        ///  get instances for the constructor.
        /// </summary>
        /// <param name="propertyItems"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="parameters"></param>
        public static T Create(IDictionary<string, object> propertyItems, IServiceProvider serviceProvider, params object[] parameters)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider, parameters);
            ReflectionHelper.SetProperties(instance, propertyItems);
            return instance;
        }
    }

    public static class Factory
    {
        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="type"/> using the constructor that best matches
        ///  the specified parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        public static object Create(Type type, params object[] parameters)
        {
            EnsureArg.IsNotNull(type, nameof(type));

            try
            {
                return Activator.CreateInstance(type, parameters);
            }
            catch (MissingMethodException)
            {
                return default;
            }
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="type"/> using the constructor that best matches
        ///  the specified parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyItems"></param>
        /// <param name="parameters"></param>
        public static object Create(Type type, IDictionary<string, object> propertyItems, params object[] parameters)
        {
            EnsureArg.IsNotNull(type, nameof(type));

            try
            {
                var instance = Activator.CreateInstance(type, parameters);
                ReflectionHelper.SetProperties(instance, propertyItems);
                return instance;
            }
            catch (MissingMethodException)
            {
                return default;
            }
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="type"/> using the serviceprovider to
        ///  get instances for the constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        public static object Create(Type type, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(type, nameof(type));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            return ActivatorUtilities.CreateInstance(serviceProvider, type);
        }

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="type"/> using the serviceprovider to
        ///  get instances for the constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyItems"></param>
        /// <param name="serviceProvider"></param>
        public static object Create(Type type, IDictionary<string, object> propertyItems, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(type, nameof(type));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            var instance = ActivatorUtilities.CreateInstance(serviceProvider, type);
            ReflectionHelper.SetProperties(instance, propertyItems);
            return instance;
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}