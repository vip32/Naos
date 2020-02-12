namespace Naos.Foundation
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ContentTypeMetadateAttribute : Attribute
    {
        public ContentTypeMetadateAttribute()
        {
            this.Value = "text/plain";
            this.IsText = true;
        }

        public string Value { get; set; }

        public string FileExtension { get; set; } // TODO: set extensions for all values

        public bool IsText { get; set; }

        public bool IsBinary
        {
            get
            {
                return !this.IsText;
            }

            set
            {
                this.IsText = !value;
            }
        }
    }
}