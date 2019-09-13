namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reflection;
    using Naos.Foundation.Utilities.Expando;

    /// <summary>
    /// Class that provides extensible properties and methods to an
    /// existing object when cast to dynamic. This
    /// dynamic object stores 'extra' properties in a dictionary or
    /// checks the actual properties of the instance passed via
    /// constructor.
    /// This class can be subclassed to extend an existing type or
    /// you can pass in an instance to extend. Properties (both
    /// dynamic and strongly typed) can be accessed through an
    /// indexer.
    /// This type allows you three ways to access its properties:
    /// Directly: any explicitly declared properties are accessible
    /// Dynamic: dynamic cast allows access to dictionary and native properties/methods
    /// Dictionary: Any of the extended properties are accessible via IDictionary interface
    /// </summary>
    [Serializable]
    public class Expando : DynamicObject, IDynamicMetaObjectProvider
    {
        private object instance;
        private PropertyInfo[] instancePropertyInfo;
        private Type instanceType; // Cached type of the instance

        /// <summary>
        /// Initializes a new instance of the <see cref="Expando"/> class.
        /// This constructor just works off the internal dictionary and any
        /// public properties of this object.
        /// Note you can subclass Expando.
        /// </summary>
        public Expando()
        {
            this.Initialize(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expando"/> class.
        /// Allows passing in an existing instance variable to 'extend'.
        /// </summary>
        /// <remarks>
        /// You can pass in null here if you don't want to
        /// check native properties and only check the Dictionary!
        /// </remarks>
        /// <param name="instance"></param>
        public Expando(object instance)
        {
            this.Initialize(instance);
        }

        /// <summary>
        /// String Dictionary that contains the extra dynamic values
        /// stored on this object/instance
        /// </summary>
        /// <remarks>Using PropertyBag to support XML Serialization of the dictionary</remarks>
        public PropertyBag Properties { get; set; } = new PropertyBag();

        private IEnumerable<PropertyInfo> InstancePropertyInfo
        {
            get
            {
                if (this.instancePropertyInfo == null && this.instance != null)
                {
                    this.instancePropertyInfo = this.instance.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                }

                return this.instancePropertyInfo;
            }
        }

        /// <summary>
        /// Convenience method that provides a string Indexer
        /// to the Properties collection AND the strongly typed
        /// properties of the object by name.
        /// // dynamic
        /// exp["Address"] = "112 nowhere lane";
        /// // strong
        /// var name = exp["StronglyTypedProperty"] as string;
        /// </summary>
        /// <remarks>
        /// The getter checks the Properties dictionary first
        /// then looks in PropertyInfo for properties.
        /// The setter checks the instance properties before
        /// checking the Properties dictionary.
        /// </remarks>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                try
                {
                    return this.Properties[key]; // try to get from properties collection first
                }
                catch (KeyNotFoundException)
                {
                    // try reflection on instanceType
                    if (this.GetProperty(this.instance, key, out var result))
                    {
                        return result;
                    }

                    throw; // nope doesn't exist
                }
            }

            set
            {
                if (this.Properties.ContainsKey(key))
                {
                    this.Properties[key] = value;
                    return;
                }

                // check instance for existance of type first
                var miArray = this.instanceType.GetMember(key, BindingFlags.Public | BindingFlags.GetProperty);
                if (miArray?.Length > 0)
                {
                    this.SetProperty(this.instance, key, value);
                }
                else
                {
                    this.Properties[key] = value;
                }
            }
        }

        /// <summary>
        /// Return both instance and dynamic names.
        /// Important to return both so JSON serialization with
        /// Json.NET works.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var property in this.GetProperties(true))
            {
                yield return property.Key;
            }
        }

        /// <summary>
        /// Try to retrieve a member by name first from instance properties
        /// followed by the collection entries.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            // first check the Properties collection for member
            if (this.Properties.Keys.Contains(binder.Name))
            {
                result = this.Properties[binder.Name];
                return true;
            }

            // Next check for Public properties via Reflection
            if (this.instance != null)
            {
                try
                {
                    return this.GetProperty(this.instance, binder.Name, out result);
                }
                catch
                {
                }
            }

            // failed to retrieve a property
            result = null;
            return false;
        }

        /// <summary>
        /// Property setter implementation tries to retrieve value from instance
        /// first then into this object
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // first check to see if there's a native property to set
            if (this.instance != null)
            {
                try
                {
                    var result = this.SetProperty(this.instance, binder.Name, value);
                    if (result)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            // no match - set or add to dictionary
            this.Properties[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Dynamic invocation method. Currently allows only for Reflection based
        /// operation (no ability to add methods dynamically).
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (this.instance != null)
            {
                try
                {
                    // check instance passed in for methods to invoke
                    if (this.InvokeMethod(this.instance, binder.Name, args, out result))
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Returns and the properties of
        /// </summary>
        /// <param name="includeInstanceProperties">if set to <c>true</c> [include instance properties].</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetProperties(bool includeInstanceProperties = false)
        {
            if (includeInstanceProperties && this.instance != null)
            {
                foreach (var property in this.InstancePropertyInfo)
                {
                    yield return new KeyValuePair<string, object>(property.Name, property.GetValue(this.instance, null));
                }
            }

            foreach (var key in this.Properties.Keys)
            {
                yield return new KeyValuePair<string, object>(key, this.Properties[key]);
            }
        }

        /// <summary>
        /// Checks whether a property exists in the Property collection
        /// or as a property on the instance
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, object> item, bool includeInstanceProperties = false)
        {
            if (this.Properties.ContainsKey(item.Key))
            {
                return true;
            }

            if (includeInstanceProperties && this.instance != null)
            {
                foreach (var property in this.InstancePropertyInfo)
                {
                    if (property.Name == item.Key)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected void Initialize(object instance)
        {
            this.instance = instance;
            if (instance != null)
            {
                this.instanceType = instance.GetType();
            }
        }

        /// <summary>
        /// Reflection Helper method to retrieve a property
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool GetProperty(object instance, string name, out object result)
        {
            if (instance == null)
            {
                instance = this;
            }

            var members = this.instanceType.GetMember(name,
                BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
            if (members?.Length > 0)
            {
                var member = members[0];
                if (member.MemberType == MemberTypes.Property)
                {
                    result = ((PropertyInfo)member).GetValue(instance, null);
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Reflection helper method to set a property value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool SetProperty(object instance, string name, object value)
        {
            if (instance == null)
            {
                instance = this;
            }

            var members = this.instanceType.GetMember(name,
                BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
            if (members?.Length > 0)
            {
                var member = members[0];
                if (member.MemberType == MemberTypes.Property)
                {
                    if (((PropertyInfo)member).CanWrite)
                    {
                        ((PropertyInfo)member).SetValue(this.instance, value, null);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reflection helper method to invoke a method
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool InvokeMethod(object instance, string name, object[] args, out object result)
        {
            if (instance == null)
            {
                instance = this;
            }

            // Look at the instanceType
            var members = this.instanceType.GetMember(name,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);
            if (members?.Length > 0)
            {
                var member = members[0] as MethodInfo;
                result = member.Invoke(this.instance, args);
                return true;
            }

            result = null;
            return false;
        }
    }
}