namespace Naos.Foundation.UnitTests.Utilities
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CSharp.RuntimeBinder;
    using Naos.Foundation;
    using Newtonsoft.Json;
    using Shouldly;
    using Xunit;

    public class ExpandoTests
    {
        [Fact]
        public void AddAndReadDynamicIndexersTest()
        {
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            ex["Address"] = "Address 123";
            ex["Contacted"] = true;

            dynamic exd = ex;

            Assert.Equal(exd.Address, ex["Address"]);
            Assert.Equal(exd.Contacted, true);
        }

        [Fact]
        public void AddAndReadDynamicPropertiesTest()
        {
            // strong typed as dynamic
            dynamic user = new User
            {
                Name = "Name",
                Email = "Name@whatsa.com"
            };
            user.entered = DateTime.Now;
            user.Company = "Company";
            user.Accesses = 10;

            Assert.Equal(user.Name, "Name");
            Assert.Equal(user.Company, "Company");
            Assert.Equal(user.Accesses, 10);
        }

        [Fact]
        public void ChildObjectTest()
        {
            dynamic duser = new User();

            // Set properties on dynamic object
            duser.Address = new Address();
            duser.Address.FullAddress = "Address 123";
            duser.Address.Phone = "111 222 333 444";

            Assert.Equal(duser.Address.Phone, "111 222 333 444");
        }

        [Fact]
        public void DynamicAccessToPropertyTest()
        {
            // Set standard properties
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            // turn into dynamic
            dynamic exd = ex;

            // Dynamic can access
            Assert.Equal(ex.Name, exd.Name);
            Assert.Equal(ex.Entered, exd.Entered);
        }

        /// <summary>
        ///     Summary method that demonstrates some
        ///     of the basic behaviors.
        ///     More specific tests are provided below
        /// </summary>
        [Fact]
        public void ExandoBasicTests()
        {
            // Set standard properties
            var ex = new User
            {
                Name = "Name",
                Email = "Name@email.com",
                Active = true
            };

            // set dynamic properties that don't exist on type
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "Address 123";
            ex["Email"] = "Name@email.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // iterate over all properties dynamic and native
            foreach (var prop in ex.GetProperties(true))
            {
                //Trace.WriteLine(prop.Key + " " + prop.Value);
            }

            // you can access plain properties both as explicit or dynamic
            ex.Name.ShouldBe(exd.Name as string);
            (exd.Name as string)?.ShouldBe(ex.Name);

            // You can access dynamic properties either as dynamic or via IDictionary
            (exd.Company as string)?.ShouldBe(ex["Company"] as string);
            (exd.Address as string)?.ShouldBe(ex["Address"] as string);

            // You can access strong type properties via the collection as well (inefficient though)
            ex.Name.ShouldBe(ex["Name"] as string);

            // dynamic can access everything
            ((decimal)ex["TotalOrderAmounts"]).ShouldBe((decimal)exd.TotalOrderAmounts); // dictionary property
        }

        [Fact]
        public void ExpandoInstanceCanAcceptExtensionMetods()
        {
            var user = new User
            {
                Email = "Name@email.com",
                Password = "Password1",
                Name = "Name1",
                Active = true
            };

            dynamic duser = user;

            duser.Phone = "111 222 333 444";

            var json = JsonConvert.SerializeObject(user);
            //Trace.WriteLine(json);
            Assert.True(!string.IsNullOrEmpty(json));

            Assert.Throws<RuntimeBinderException>(() => duser.Dump()); // does not contain a definition for 'Dump'
        }

        [Fact]
        public void ExpandoMixinTest1()
        {
            // have Expando work on Addresses
            var user = new User(new Address());

            // cast to dynamicAccessToPropertyTest
            dynamic duser = user;

            // Set strongly typed properties
            duser.Email = "Name@email.com";
            user.Password = "Password1";

            // Set properties on address object
            duser.Address = "Address 123";
            duser.Phone = "111 222 333 444";

            // set dynamic properties
            duser.NonExistantProperty = "NonExistantProperty1";

            // shows default value Address.Phone value
            //Trace.WriteLine(string.Format("phone:{0}", duser.Phone as string));
            Assert.Equal("111 222 333 444", user["Phone"]);
            Assert.Equal("111 222 333 444", duser.Phone);
        }

        [Fact]
        public void InheritedExpandoInstanceSerializesEverything()
        {
            var user = new SpecialUser
            {
                Name = "name",
                Country = "country" // special user property
            };

            var json = JsonConvert.SerializeObject(user);

            //Trace.WriteLine(json);
            Assert.True(!string.IsNullOrEmpty(json));
            //Assert.IsTrue(json.Contains("Name") || json.Contains("name"));
            Assert.True(json.Contains("Country", StringComparison.OrdinalIgnoreCase) || json.Contains("country", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void IterateOverDynamicPropertiesTest()
        {
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            dynamic exd = ex;
            exd.Company = "Company";
            exd.Accesses = 10;

            // Dictionary pseudo implementation
            ex["Count"] = 10;
            ex["Type"] = "MyType";

            // Dictionary Count - 2 dynamic props added
            Assert.True(ex.Properties.Count == 4);

            // iterate over all properties
            foreach (KeyValuePair<string, object> prop in exd.GetProperties(true))
            {
                //Trace.WriteLine(prop.Key + " " + prop.Value);
            }
        }

        [Fact]
        public void MixInObjectInstanceTest()
        {
            // Create expando an mix-in second objectInstanceTest
            var ex = new ExpandoInstance(new Address())
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            // create dynamic
            dynamic exd = ex;

            // values should show Addresses initialized values (not null)
            //Trace.WriteLine(string.Format("address:{0}", exd.FullAddress as string));
            //Trace.WriteLine(string.Format("email:{0}", exd.Email as string));
            //Trace.WriteLine(string.Format("phone:{0}", exd.Phone as string));
        }

        [Fact]
        public void PropertyAsIndexerTest()
        {
            // Set standard properties
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            Assert.Equal(ex.Name, ex["Name"]);
            Assert.Equal(ex.Entered, ex["Entered"]);
        }

        [Fact]
        public void ToExandoBasicTests()
        {
            // Set standard properties
            var ex = new User
            {
                Name = "Name",
                Email = "Name@email.com",
                Active = true
            };

            // set dynamic properties that don't exist on type
            ex.ToExpando().Entered = DateTime.Now;
            ex.ToExpando().Company = "Company";
            ex.ToExpando().Accesses = 10;

            // you can access plain properties both as explicit or dynamic
            Assert.Equal(ex.Name, ex.ToExpando().Name);

            // You can access dynamic properties either as dynamic or via IDictionary
            Assert.Equal(ex.ToExpando().Company, ex["Company"] as string);

            // You can access strong type properties via the collection as well (inefficient though)
            Assert.Equal(ex.Name, ex["Name"] as string);

            // dynamic can access everything
            Assert.Equal(ex.Name, ex.ToExpando().Name); // native property
        }

        [Fact]
        public void TwoWayJsonSerializeExpandoTyped()
        {
            // Set standard properties
            var ex = new User
            {
                Name = "Name",
                Email = "Name@email.com",
                Password = "Password1",
                Active = true
            };

            // set dynamic properties
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "Address 123";
            ex["Email"] = "Name@email.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // *** Should serialize both standard properties and dynamic properties
            var json = JsonConvert.SerializeObject(ex);
            Assert.True(!string.IsNullOrEmpty(json));
            //Trace.WriteLine("*** Serialized Native object:");
            //Trace.WriteLine(json);

#pragma warning disable xUnit2009 // Do not use boolean check to check for substrings
#pragma warning disable CA1307 // Specify StringComparison
            Assert.True(json.Contains("Name")); // standard
            Assert.True(json.Contains("Company")); // dynamic

            // *** Now deserialize the JSON back into object to
            // *** check for two-way serialization
            var user2 = JsonConvert.DeserializeObject<User>(json);
            Assert.NotNull(user2);
            json = JsonConvert.SerializeObject(user2);
            Assert.True(!string.IsNullOrEmpty(json));
            //Trace.WriteLine("*** De-Serialized User2 object:");
            //Trace.WriteLine(json);

            Assert.True(json.Contains("Name")); // standard
            Assert.True(json.Contains("Company")); // dynamic
#pragma warning restore CA1307 // Specify StringComparison
#pragma warning restore xUnit2009 // Do not use boolean check to check for substrings
        }

        [Fact]
        public void TwoWayXmlSerializeExpandoTyped()
        {
            // Set standard properties
            var ex = new User { Name = "Name", Active = true };

            // set dynamic properties
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "Address 123";
            ex["Email"] = "Name@email.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // Serialize creates both static and dynamic properties
            // dynamic properties are serialized as a 'collection'
            string xml;
            Naos.Foundation.Utilities.Expando.SerializationHelper.SerializeObject(exd, out xml);
            //Trace.WriteLine("*** Serialized Dynamic object:");
            //Trace.WriteLine(xml);

#pragma warning disable xUnit2009 // Do not use boolean check to check for substrings
#pragma warning disable CA1307 // Specify StringComparison
            Assert.True(xml.Contains("Name")); // static
            Assert.True(xml.Contains("Company")); // dynamic

            // Serialize
            var user2 = Naos.Foundation.Utilities.Expando.SerializationHelper.DeSerializeObject(xml, typeof(User));
            Naos.Foundation.Utilities.Expando.SerializationHelper.SerializeObject(exd, out xml);
            //Trace.WriteLine(xml);

            Assert.True(xml.Contains("Name")); // static
            Assert.True(xml.Contains("Company")); // dynamic
#pragma warning restore CA1307 // Specify StringComparison
#pragma warning restore xUnit2009 // Do not use boolean check to check for substrings
        }

        //[Fact]
        //public void ExpandoObjectJsonTest()
        //{
        //    dynamic ex = new ExpandoObject();
        //    ex.Name = "Name";
        //    ex.Entered = DateTime.Now;
        //    ex.Address = "Address 123";
        //    ex.Contacted = true;
        //    ex.Count = 10;
        //    ex.Completed = DateTime.Now.AddHours(2);

        //    string json = JsonConvert.SerializeObject(ex, Formatting.Indented);
        //    Assert.IsTrue(!string.IsNullOrEmpty(json));
        //    System.Diagnostics.Trace.WriteLine(json);
        //}

        [Fact]
        public void UserExampleTest()
        {
            // Set strongly typed properties
            var user = new User
            {
                Email = "Name@email.com",
                Password = "Password1",
                Name = "Name1",
                Active = true
            };

            // Now add dynamic properties
            dynamic duser = user;
            duser.Entered = DateTime.Now;
            duser.Accesses = 1;

            // you can also add dynamic props via indexer
            user["NickName"] = "NickName1";
            duser["WebSite"] = "http://www.example.com";

            // Access strong type through dynamic ref
            Assert.Equal(user.Name, duser.Name);
            // Access strong type through indexer
            Assert.Equal(user.Password, user["Password"]);
            // access dyanmically added value through indexer
            Assert.Equal(duser.Entered, user["Entered"]);
            // access index added value through dynamic
            Assert.Equal(user["NickName"], duser.NickName);

            // loop through all properties dynamic AND strong type properties (true)
            foreach (var prop in user.GetProperties(true))
            {
                var val = prop.Value;
                if (val == null)
                {
                    val = "null";
                }

                //Trace.WriteLine(prop.Key + ": " + val);
            }
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class ExpandoInstance : Expando
    {
        public ExpandoInstance()
        {
        }

        public ExpandoInstance(object instance)
            : base(instance)
        {
        }

        public string Name { get; set; }

        public DateTime Entered { get; set; }
    }

    public class Address
    {
        public Address()
        {
            this.FullAddress = "Address 123";
            this.Phone = "123 456 7890";
            this.Email = "Name@email.com";
        }

        public string FullAddress { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }

    public class User : Expando
    {
        public User()
        {
        }

        // only required if you want to mix in seperate instance
        public User(object instance)
            : base(instance)
        {
        }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public DateTime? ExpiresOn { get; set; }
    }

    public class SpecialUser : User
    {
        public string Country { get; set; }
    }

    public class VerySpecialUser : SpecialUser
    {
        public string Title { get; set; }
    }
}