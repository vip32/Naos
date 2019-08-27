namespace Naos.Foundation
{
    using System;

    public static class ContentTypeExtensions
    {
        public static ContentType FromValue(string value, ContentType @default = ContentType.TEXT)
        {
            if (string.IsNullOrEmpty(value))
            {
                return @default;
            }

            foreach (var enumValue in Enum.GetValues(typeof(ContentType)))
            {
                Enum.TryParse(enumValue.ToString(), true, out ContentType contentType);
                var metaDataValue = contentType.GetAttributeValue<ContentTypeMetadata, string>(x => x.Value);
                if (metaDataValue != null && metaDataValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return contentType;
                }
            }

            return @default;
        }

        public static ContentType FromExtension(string extension, ContentType @default = ContentType.TEXT)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return @default;
            }

            foreach (var enumValue in Enum.GetValues(typeof(ContentType)))
            {
                Enum.TryParse(enumValue.ToString(), true, out ContentType contentType);
                var metaDataValue = contentType.GetAttributeValue<ContentTypeMetadata, string>(x => x.FileExtension);
                if (metaDataValue != null)
                {
                    // compare the attribute value with the extension
                    if (metaDataValue.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return contentType;
                    }
                }
                else
                {
                    // compare the enum value with the extension
                    if (enumValue.ToString().SafeEquals(extension))
                    {
                        return contentType;
                    }
                }
            }

            return @default;
        }

        public static ContentType FromFileName(string fileName, ContentType @default = ContentType.TEXT)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return @default;
            }

            return FromExtension(fileName.SliceFromLast("."), @default);
        }

        public static string ToValue(this ContentType contentType)
        {
            var metadata = GetMetadata(contentType);
            return (metadata != null) ? ((ContentTypeMetadata)metadata).Value : contentType.ToString();
        }

        public static bool IsText(this ContentType contentType)
        {
            var metadata = GetMetadata(contentType);
            return (metadata != null) ? ((ContentTypeMetadata)metadata).IsText : true;
        }

        public static bool IsBinary(this ContentType contentType)
        {
            var metadata = GetMetadata(contentType);
            return (metadata != null) ? ((ContentTypeMetadata)metadata).IsBinary : false;
        }

        public static string FileExtension(this ContentType contentType)
        {
            var metadata = GetMetadata(contentType);
            return (metadata != null && !string.IsNullOrEmpty(((ContentTypeMetadata)metadata).FileExtension))
                ? ((ContentTypeMetadata)metadata).FileExtension
                : contentType.ToString().ToLower();
        }

        private static object GetMetadata(ContentType contentType)
        {
            var type = contentType.GetType();
            var info = type.GetMember(contentType.ToString());
            if ((info != null) && (info.Length > 0))
            {
                var attrs = info[0].GetCustomAttributes(typeof(ContentTypeMetadata), false);
                if ((attrs != null) && (attrs.Length > 0))
                {
                    return attrs[0];
                }
            }

            return null;
        }
    }
}