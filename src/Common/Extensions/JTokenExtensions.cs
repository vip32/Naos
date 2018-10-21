namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public static class JTokenExtensions
    {
        public static JToken AsJToken(this object source, bool useContractResolver = false)
        {
            if (source == null)
            {
                return default(JToken);
            }

            if (!useContractResolver)
            {
                return JToken.FromObject(
                    source,
                    new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
            }

            return JToken.FromObject(source, new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableInterface = false,
                    IgnoreSerializableAttribute = false
                }
            });
        }

        public static T AsObject<T>(this JToken source)
        {
            if (source == null)
            {
                return default(T);
            }

            return source.ToObject<T>();
        }

        public static bool IsNullOrEmpty(this JToken source)
        {
            return (source == null)
                   || (source.Type == JTokenType.Array && !source.HasValues)
                   || (source.Type == JTokenType.Object && !source.HasValues)
                   || (source.Type == JTokenType.String && source.ToString().Length == 0)
                   || (source.Type == JTokenType.Null);
        }

        /// <summary>
        /// Gets the value by path. If the path does not exist it will return null;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The document.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static T GetValueByPath<T>(this JToken source, string path)
        {
            if (path.IsNullOrEmpty())
            {
                return default(T);
            }

            if (path.StartsWith("jsonpath:", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Replace("jsonpath:", string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            try
            {
                return source.SelectToken(path).Value<T>();
            }
            catch (Exception)
            {
                // argumentnullexception, jsonexception
                return default(T);
            }
        }

        /// <summary>
        /// Gets the values by path. If the path does not exist it will return null;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The document.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetValuesByPath<T>(this JToken source, string path)
        {
            if (path.IsNullOrEmpty())
            {
                return default(IEnumerable<T>);
            }

            if (path.StartsWith("jsonpath:", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Replace("jsonpath:", string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            try
            {
                var results = source.SelectTokens(path);
                return results.NullToEmpty().Select(r => r.Value<T>());
            }
            catch (Exception)
            {
                // argumentnullexception, jsonexception
                return default(IEnumerable<T>);
            }
        }

        /// <summary>
        /// Sets the value by path. If the path does not exist it will be created recursivly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The document.</param>
        /// <param name="path">The path.</param>
        /// <param name="value">The value.</param>
        public static void SetValueByPath<T>(this JToken source, string path, T value)
        {
            if (source.IsNullOrEmpty() || path.IsNullOrEmpty())
            {
                return;
            }

            if (path.StartsWith("jsonpath:", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Replace("jsonpath:", string.Empty);
            }

            var values = source.SelectTokens(path).OfType<JValue>();
            if (!values.IsNullOrEmpty())
            {
                // update existing prop
                foreach (var prop in values)
                {
                    prop.Value = value;
                }
            }
            else
            {
                // insert new prop
                SetNewValueByToken(source, path, new JValue(value));
            }
        }

        public static string GetStringPropertyByToken(this JToken source, string path)
        {
            try
            {
                return (string)source.SelectToken(path);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static int? GetIntPropertyByToken(this JToken source, string path)
        {
            try
            {
                var value = (string)source.SelectToken(path);

                return ConvertValue<int>(value);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static double? GetDoublePropertyByToken(this JToken source, string path)
        {
            try
            {
                var value = (string)source.SelectToken(path);

                return ConvertValue<double>(value);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static decimal? GetDecimalPropertyByToken(this JToken source, string path, IFormatProvider provider = null)
        {
            try
            {
                var value = (string)source.SelectToken(path);
                return ConvertValue<decimal>(value, provider);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static DateTime? GetDateTimePropertyByToken(this JToken source, string path, IFormatProvider provider = null)
        {
            try
            {
                var value = (string)source.SelectToken(path);
                return ConvertValue<DateTime>(value, provider);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool? GetBoolPropertyByToken(this JToken source, string path)
        {
            try
            {
                var value = (string)source.SelectToken(path);
                value = value?.Replace("1", "true", StringComparison.OrdinalIgnoreCase).Replace("0", "false", StringComparison.OrdinalIgnoreCase);
                value = value?.Replace("ja", "true", StringComparison.OrdinalIgnoreCase).Replace("nein", "false", StringComparison.OrdinalIgnoreCase);
                value = value?.Replace("yes", "true", StringComparison.OrdinalIgnoreCase).Replace("no", "false", StringComparison.OrdinalIgnoreCase);
                return ConvertValue<bool>(value);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static string AddOrUpdatePath<T>(string json, string path, T newValue)
        {
            return JToken.Parse(json).AddOrUpdatePath(path, newValue).ToString();
        }

        public static JToken AddOrUpdatePath<T>(this JToken source, string path, T newValue)
        {
            if (source == null || path == null)
            {
                return source;
            }

            string[] tokenPaths = path.Split('.');

            // make sure all tokens exist
            var tokenPointer = source;
            foreach (var tokenPath in tokenPaths)
            {
                if (tokenPointer.SelectToken(tokenPath) == null)
                {
                    var obj = (JObject)tokenPointer;
                    if (tokenPath.Contains('['))
                    {
                        var replacer = tokenPath.Substring(tokenPath.IndexOf('['));
                        var arrayPropName = tokenPath.Replace(replacer, string.Empty);

                        if (obj.SelectToken(arrayPropName) == null)
                        {
                            obj.Add(arrayPropName, new JArray(new JObject()));
                        }
                        else
                        {
                            var arr = (JArray)obj.SelectToken(arrayPropName);
                            arr.Add(new JObject());
                        }
                    }
                    else
                    {
                        obj.Add(tokenPath, new JObject());
                    }
                }

                tokenPointer = tokenPointer.SelectToken(tokenPath);
            }

            // add or update tokens
            foreach (var value in source.SelectTokens(path).ToList())
            {
                var token = !newValue.IsDefault() ? JToken.FromObject(newValue) : null;

                if (value == source)
                {
                    source = token;
                }
                else
                {
                    value.Replace(token);
                }
            }

            return source;
        }

        private static void SetNewValueByToken(JToken source, string path, JToken value)
        {
            var pathParts = path.Split('.').Reverse();
            var pathPartsFull = new List<string>();

            // create a list of full paths
            // a.b.c.d > a , a.b , a.b.c , a.b.c.d
            var skip = 0;
            foreach (var pathPart in pathParts)
            {
                pathPartsFull.Add(pathParts.Skip(skip).Reverse().ToString("."));
                skip++;
            }

            pathPartsFull.Reverse();

            foreach (var pathPartFull in pathPartsFull)
            {
                // check all paths which need creation
                var token = source.SelectToken(pathPartFull);
                if (token == null)
                {
                    // find previous token so new token can be added
                    var prevToken = source.SelectToken(pathPartFull.SubstringTillLast(".")) ?? source;
                    if (pathPartFull != pathPartsFull.LastOrDefault())
                    {
                        // non last tokens contain just jobjects, so value can be added later
                        prevToken[pathPartFull.Split('.').Last()] = new JObject();
                    }
                    else
                    {
                        // last property contains value token
                        prevToken[pathPartFull.Split('.').Last()] = value;
                    }
                }
            }
        }

        private static T? ConvertValue<T>(string value, IFormatProvider provider = null)
            where T : struct
        {
            return !string.IsNullOrEmpty(value) ? (T?)Convert.ChangeType(value, typeof(T), provider) : null;
        }
    }
}
