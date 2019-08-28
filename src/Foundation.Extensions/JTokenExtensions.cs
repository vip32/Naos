namespace Naos.Foundation
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
                return default;
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
                ContractResolver = new CamelCasePropertyNamesContractResolver
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
                return default;
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

        public static JToken EmptyToNull(this JToken source)
        {
            if (source.IsNullOrEmpty())
            {
                return null;
            }

            return source;
        }

        /// <summary>
        /// Gets the value by path. If the path does not exist it will return null;.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The document.</param>
        /// <param name="path">The path.</param>
        public static T GetValueByPath<T>(this JToken source, string path)
        {
            if (source.IsNullOrEmpty() || path.IsNullOrEmpty())
            {
                return default;
            }

            try
            {
                return source.SelectToken(path).Value<T>();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // argumentnullexception, jsonexception
                return default;
            }
        }

        /// <summary>
        /// Gets the values by path. If the path does not exist it will return null;.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The document.</param>
        /// <param name="path">The path.</param>
        public static IEnumerable<T> GetValuesByPath<T>(this JToken source, string path)
        {
            if (path.IsNullOrEmpty())
            {
                return default;
            }

            try
            {
                var results = source.SelectTokens(path);
                return results.Safe().Select(r => r.Value<T>());
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // argumentnullexception, jsonexception
                return default;
            }
        }

        /// <summary>
        /// Sets the value by path. If the path does not exist it will be created recursivly.
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

        public static string GetStringPropertyByPath(this JToken source, string path)
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

        public static int? GetIntPropertyByPath(this JToken source, string path)
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

        public static double? GetDoublePropertyByPath(this JToken source, string path)
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

        public static decimal? GetDecimalPropertyByPath(this JToken source, string path, IFormatProvider provider = null)
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

        public static DateTime? GetDateTimePropertyByPath(this JToken source, string path, IFormatProvider provider = null)
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

        public static bool? GetBoolPropertyByPath(this JToken source, string path)
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

        public static JToken AddOrUpdateByPath<T>(this JToken source, string path, T newValue)
        {
            if (source.IsNullOrEmpty() || path.IsNullOrEmpty())
            {
                return source;
            }

            var pathParts = path.Split('.');

            // make sure all tokens exist
            var tokenPointer = source;
            foreach (var pathPart in pathParts)
            {
                if (tokenPointer.SelectToken(pathPart) == null)
                {
                    var obj = (JObject)tokenPointer;
                    if (pathPart.Contains('['))
                    {
                        var replacer = pathPart.Substring(pathPart.IndexOf('['));
                        var arrayPropName = pathPart.Replace(replacer, string.Empty);

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
                        obj.Add(pathPart, new JObject());
                    }
                }

                tokenPointer = tokenPointer.SelectToken(pathPart);
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

        public static JToken RemovePropertyByPath(this JToken source, string path)
        {
            if (source.IsNullOrEmpty() || path.IsNullOrEmpty() || !path.ContainsAny(new[] { "." }))
            {
                return source;
            }

            var propertyName = path.SliceFromLast(".");
            JToken target;
            try
            {
                target = source.SelectToken(path.SliceTillLast("."));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return source;
            }

            if (!target.IsNullOrEmpty() && !propertyName.IsNullOrEmpty())
            {
                target.RemoveProperty(propertyName);
            }

            return source;
        }

        public static JToken RemoveProperty(this JToken source, string propertyName)
        {
            return source.RemoveProperties(new[] { propertyName });
        }

        public static JToken RemoveProperties(this JToken source, string[] propertyNames)
        {
            var container = source as JContainer;
            if (container == null || propertyNames.IsNullOrEmpty())
            {
                return source;
            }

            var tokens = new List<JToken>();
            foreach (var token in container.Children())
            {
                if (token is JProperty p && propertyNames.Contains(p.Name))
                {
                    tokens.Add(token);
                }

                token.RemoveProperties(propertyNames);
            }

            foreach (var token in tokens)
            {
                token.Remove();
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
                    var prevToken = source.SelectToken(pathPartFull.SliceTillLast(".")) ?? source;
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
