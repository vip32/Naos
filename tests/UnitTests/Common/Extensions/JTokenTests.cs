namespace Naos.Core.UnitTests.Common
{
    using System.Linq;
    using Naos.Core.Common;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class JTokenTests
    {
        private readonly string json = @"
{
    ""myCats"": [
        {
            ""name"": ""Hero"",
            ""age"": 5,
            ""agestring"": ""5"",
            ""alive"": true,
            ""alive2"": ""1"",
            ""alive3"": ""ja"",
            ""alive4"": ""Ja"",
            ""color"": [""silver""]
        },
        {
            ""name"": ""Euro"",
            ""age"": 2,
            ""alive"": true,
            ""color"": [""brown"", ""white"", ""black""]
        }
    ]
}
";

        [Fact]
        public void Various_GetValueByPath()
        {
            var doc = JToken.Parse(this.json);

            Assert.Equal("Hero", doc.GetValueByPath<string>("myCats[0].name"));
            Assert.Equal("Hero", doc.GetValueByPath<string>("jsonpath:myCats[0].name"));
            Assert.Null(doc.GetValueByPath<string>("myCats[0].unknown"));
            Assert.Equal(5, doc.GetValueByPath<int>("myCats[0].age"));
            Assert.Null(doc.GetValueByPath<int?>("myCats[0].noage"));
            Assert.Equal(5, doc.GetValueByPath<int?>("myCats[0].agestring"));
            Assert.True(doc.GetValueByPath<bool>("myCats[0].alive"));
            //Assert.True(doc.GetValueByPath<bool>("myCats[0].alive2"));
            //Assert.True(doc.GetValueByPath<bool>("myCats[0].alive3"));
            //Assert.True(doc.GetValueByPath<bool>("myCats[0].alive4"));

            Assert.Equal(2, doc.GetValuesByPath<string>("myCats[?(@.age >= 1)].name").Count());
            Assert.Single(doc.GetValuesByPath<string>("myCats[?(@.name == 'Hero')].age"));

            doc = JObject.Parse("{ \"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users\",\"value\":[] }");
            Assert.Null(doc.GetValueByPath<string>("values[0]"));
        }

        [Fact]
        public void Various_GetByToken()
        {
            var doc = JToken.Parse(this.json);

            Assert.Equal("Hero", doc.GetStringPropertyByPath("myCats[0].name"));
            Assert.Equal("Hero", doc.GetStringPropertyByPath("myCats[0].name"));
            Assert.Null(doc.GetStringPropertyByPath("myCats[0].unknown"));
            Assert.Equal(5, doc.GetIntPropertyByPath("myCats[0].age"));
            Assert.Null(doc.GetIntPropertyByPath("unknown"));
            Assert.Equal(true, doc.GetBoolPropertyByPath("myCats[0].alive"));
            Assert.Null(doc.GetBoolPropertyByPath("unknown"));
            Assert.Equal(true, doc.GetBoolPropertyByPath("myCats[0].alive"));
            Assert.Equal(true, doc.GetBoolPropertyByPath("myCats[0].alive2"));
            Assert.Equal(true, doc.GetBoolPropertyByPath("myCats[0].alive3"));
            Assert.Equal(true, doc.GetBoolPropertyByPath("myCats[0].alive4"));
        }

        [Fact]
        public void Various_RemoveProperty()
        {
            var doc = JToken.Parse(this.json);

            Assert.Equal("Hero", doc.GetStringPropertyByPath("myCats[0].name"));
            doc.RemovePropertyByPath("myCats[0].name");
            Assert.Null(doc.GetValueByPath<string>("myCats[0].name"));

            doc.RemovePropertyByPath("myCats[0].unknown");
            Assert.Null(doc.GetValueByPath<string>("myCats[0].unknown"));

            Assert.Equal("5", doc.GetStringPropertyByPath("myCats[0].agestring"));
            doc.RemoveProperty("agestring");
            Assert.Null(doc.GetValueByPath<string>("myCats[0].agestring"));

            doc.RemoveProperty("unknown2");
            Assert.Null(doc.GetValueByPath<string>("myCats[0].unknown2"));
        }

        [Fact]
        public void Various_SetByPath()
        {
            var doc = JToken.Parse(this.json);

            doc.SetValueByPath("myCats[0].name", "NewHero");
            doc.SetValueByPath("myCats[0].age", 10);
            doc.SetValueByPath("newrootprop", "testroot");
            doc.SetValueByPath("myCats[0].newprop", "test1");
            doc.SetValueByPath("myCats[0].a.b.c", "cccc"); // add completly new value in hierarchy
            doc.SetValueByPath("myCats[0].a.b.d", "dddd"); // add completly new value in hierarchy
            doc.SetValueByPath("myCats[0].newint", 11);
            doc.SetValueByPath("myCats[1].newprop", "test2");
            doc.SetValueByPath("myCats[1].newint", 12);
            doc.AddOrUpdateByPath<string>("myCats[1].name", null);
            doc.AddOrUpdateByPath<int?>("myCats[1].age", null);

            Assert.Equal("NewHero", doc.GetValueByPath<string>("myCats[0].name"));
            Assert.Equal("NewHero", doc.GetValueByPath<string>("jsonpath:myCats[0].name"));
            Assert.Equal(10, doc.GetValueByPath<int>("myCats[0].age"));
            Assert.Equal("test1", doc.GetValueByPath<string>("myCats[0].newprop"));
            Assert.Equal("testroot", doc.GetValueByPath<string>("newrootprop"));
            Assert.Equal("testroot", doc.GetValueByPath<string>(".newrootprop"));
            Assert.Equal(11, doc.GetValueByPath<int>("myCats[0].newint"));
            Assert.Equal("test2", doc.GetValueByPath<string>("myCats[1].newprop"));
            Assert.Equal(12, doc.GetValueByPath<int>("myCats[1].newint"));
            Assert.Null(doc.GetValueByPath<string>("myCats[1].name"));
            Assert.Null(doc.GetValueByPath<int?>("myCats[1].age"));

            //System.Diagnostics.Trace.WriteLine(doc);
        }

        [Fact]
        public void JTokenToObjectConversion_Tests()
        {
            var o = new StubObject
            {
                Firstname = "test1",
                Lastname = "test1"
            };

            var jtoken = o.AsJToken();
            var co = jtoken.AsObject<StubObject>();

            Assert.NotNull(co);
            Assert.Equal(o.Firstname, co.Firstname);
            Assert.Equal(o.Lastname, co.Lastname);

            jtoken = null;
            Assert.Null(jtoken.AsObject<StubObject>());
        }

        private class StubObject
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }
        }
    }
}
