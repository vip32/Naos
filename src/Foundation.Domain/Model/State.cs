namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class State
    {
        [JsonProperty(nameof(CreatedBy))]
        public string CreatedBy { get; private set; }

        [JsonProperty(nameof(CreatedDate))]
        public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;

        //[JsonProperty(nameof(CreatedEpoch))]
        //public long CreatedEpoch
        //{
        //    get { return this.CreatedDate.ToEpoch(); }
        //    set { this.CreatedDate = Extensions.FromEpoch(value); }
        //}

        [JsonProperty(nameof(CreatedDescription))]
        public string CreatedDescription { get; private set; }

        [JsonProperty(nameof(UpdatedBy))]
        public string UpdatedBy { get; private set; }

        [JsonProperty(nameof(UpdatedDate))]
        public DateTimeOffset UpdatedDate { get; private set; } = DateTimeOffset.UtcNow;

        //[JsonProperty(nameof(UpdatedEpoch))]
        //public long UpdatedEpoch
        //{
        //    get { return this.UpdatedDate.ToEpoch(); }
        //    set { this.UpdatedDate = Extensions.FromEpoch(value); }
        //}

        [JsonProperty(nameof(UpdatedDescription))]
        public string UpdatedDescription { get; set; }

        [JsonProperty(nameof(UpdatedReasons))]
        public IEnumerable<string> UpdatedReasons { get; private set; }

        [JsonProperty(nameof(ExpiredBy))]
        public string ExpiredBy { get; set; }

        [JsonProperty(nameof(ExpiredDate))]
        public DateTimeOffset? ExpiredDate { get; set; }

        //[JsonProperty(nameof(ExpiredEpoch))]
        //public long? ExpiredEpoch
        //{
        //    get { return this.ExpiredDate.ToEpoch(); }
        //    set { this.ExpiredDate = Extensions.FromEpoch(value); }
        //}

        [JsonProperty(nameof(ExpiredDescription))]
        public string ExpiredDescription { get; set; }

        [JsonProperty(nameof(Deactivated))]
        public bool? Deactivated { get; private set; }

        [JsonProperty(nameof(DeactivatedReasons))]
        public IEnumerable<string> DeactivatedReasons { get; private set; }

        [JsonProperty(nameof(DeactivatedBy))]
        public string DeactivatedBy { get; private set; }

        [JsonProperty(nameof(DeactivatedDate))]
        public DateTimeOffset? DeactivatedDate { get; private set; }

        //[JsonProperty(nameof(DeactivatedEpoch))]
        //public long? DeactivatedEpoch
        //{
        //    get { return this.DeactivatedDate.ToEpoch(); }
        //    set { this.DeactivatedDate = Extensions.FromEpoch(value); }
        //}

        [JsonProperty(nameof(DeactivatedDescription))]
        public string DeactivatedDescription { get; set; }

        [JsonProperty(nameof(Deleted))]
        public bool? Deleted { get; private set; }

        [JsonProperty(nameof(DeletedBy))]
        public string DeletedBy { get; private set; }

        [JsonProperty(nameof(DeletedDate))]
        public DateTimeOffset? DeletedDate { get; private set; }

        //[JsonProperty(nameof(DeletedEpoch))]
        //public long? DeletedEpoch
        //{
        //    get { return this.DeletedDate.ToEpoch(); }
        //    set { this.DeletedDate = Extensions.FromEpoch(value); }
        //}

        [JsonProperty(nameof(DeletedReason))]
        public string DeletedReason { get; private set; }

        [JsonProperty(nameof(DeletedDescription))]
        public string DeletedDescription { get; set; }

        [JsonProperty(nameof(LastAccessedBy))]
        public string LastAccessedBy { get; set; }

        [JsonProperty(nameof(LastAccessedDate))]
        public DateTimeOffset? LastAccessedDate { get; set; }

        //[JsonProperty(nameof(LastAccessedEpoch))]
        //public long? LastAccessedEpoch
        //{
        //    get { return this.LastAccessedDate.ToEpoch(); }
        //    set { this.LastAccessedDate = Extensions.FromEpoch(value); }
        //}

        [JsonProperty(nameof(LastAccessedDescription))]
        public string LastAccessedDescription { get; set; }

        public DateTimeOffset? LastActionDate =>
            new List<DateTimeOffset?> { this.CreatedDate, this.UpdatedDate, this.DeletedDate ?? default(DateTimeOffset?)/*, this.LastAccessedDate*/ }
            .Where(d => d != null).Safe().Max();

        /// <summary>
        /// Gets a value indicating whether determines whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deactivated; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDeactivated =>
            (this.Deactivated != null && (bool)this.Deactivated) || !this.DeactivatedReasons.IsNullOrEmpty();

        /// <summary>
        /// Gets a value indicating whether determines whether this instance is deleted.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsDeleted() =>
            (this.Deleted != null && (bool)this.Deleted) || !this.DeletedReason.IsNullOrEmpty();

        /// <summary>
        ///     Sets the deactivated information.
        /// </summary>
        /// <param name="by">Name of the deactivator.</param>
        /// <param name="reason">The reason.</param>
        public virtual void SetDeactivated(string by = null, string reason = null)
        {
            this.DeactivatedDate = DateTimeOffset.UtcNow;
            this.SetUpdated(by);
            this.Deactivated = true;

            if (!by.IsNullOrEmpty())
            {
                this.DeactivatedBy = by;
            }

            if (!reason.IsNullOrEmpty())
            {
                if (this.DeactivatedReasons.IsNullOrEmpty())
                {
                    this.DeactivatedReasons = new List<string>();
                }

                this.DeactivatedReasons = this.DeactivatedReasons.Concat(new[]
                {
                    $"{by}: ({this.DeactivatedDate.Value.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim()
                });
            }
        }

        /// <summary>
        /// Sets the created information.
        /// </summary>
        /// <param name="by">Name of the account of the creater.</param>
        /// <param name="description">The description for the creation.</param>
        public virtual void SetCreated(string by = null, string description = null)
        {
            this.CreatedDate = DateTimeOffset.UtcNow;
            if (!by.IsNullOrEmpty())
            {
                this.CreatedBy = by;
            }

            if (!description.IsNullOrEmpty())
            {
                this.CreatedDescription = description;
            }
        }

        /// <summary>
        ///     Sets the updated information.
        /// </summary>
        /// <param name="by">Name of the account of the updater.</param>
        /// <param name="reason">The reason of the update.</param>
        public virtual void SetUpdated(string by = null, string reason = null)
        {
            this.UpdatedDate = DateTimeOffset.UtcNow;

            if (!by.IsNullOrEmpty())
            {
                this.UpdatedBy = by;
            }

            if (!reason.IsNullOrEmpty())
            {
                if (this.UpdatedReasons.IsNullOrEmpty())
                {
                    this.UpdatedReasons = new List<string>();
                }

                this.UpdatedReasons = this.UpdatedReasons.Concat(new[]
                {
                    $"{by}: ({this.UpdatedDate.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim()
                });
            }
        }

        /// <summary>
        ///     Sets the deleted information.
        /// </summary>
        /// <param name="by">Name of the deleter.</param>
        /// <param name="reason">The reason.</param>
        public virtual void SetDeleted(string by = null, string reason = null)
        {
            this.Deleted = true;
            this.DeletedDate = DateTimeOffset.UtcNow;
            this.UpdatedDate = this.DeletedDate.Value;

            if (!by.IsNullOrEmpty())
            {
                this.DeletedBy = by;
            }

            if (!reason.IsNullOrEmpty())
            {
                this.DeletedReason = $"{by}: ({this.DeletedDate.Value.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim();
            }
        }
    }
}
