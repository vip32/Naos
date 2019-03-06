namespace Naos.Core.Common
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.Extensions.DependencyInjection;

#pragma warning disable SA1402 // File may only contain a single class
    /// <summary>
    /// Creates instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type with a parameterless constructor.</typeparam>
    public static class Factory<T>
        where T : class, new()
    {
        /// <summary>
        /// Create an instance by using compiled lambda expressions
        /// https://vagifabilov.wordpress.com/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/
        /// </summary>
        private static readonly Func<T> CreateFunc =
            Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

        /// <summary>
        /// Creates an instance of type <typeparamref name="T"/> by calling it's parameterless constructor.
        /// </summary>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public static T Create() => CreateFunc(); // without ctor, fast

        //public static object Create(Type type) => Activator.CreateInstance(type);

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the constructor that best matches
        ///  the specified parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Create(Type type, params object[] parameters)
            => Activator.CreateInstance(type, parameters) as T;

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="T"/> using the serviceprovider to
        ///  get instances for the constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static T Create(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<T>(serviceProvider);
    }

    public static class Factory
    {
        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="type"/> using the constructor that best matches
        ///  the specified parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object Create(Type type, params object[] parameters)
            => Activator.CreateInstance(type, parameters);

        /// <summary>
        ///  Creates an instance of the specified type <typeparamref name="type"/> using the serviceprovider to
        ///  get instances for the constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object Create(Type type, IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance(serviceProvider, type);
    }
#pragma warning restore SA1402 // File may only contain a single class
}