namespace Naos.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Naos.Core.Common;

    public class State
    {
        public string CreatedBy { get; private set; }

        public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;

        //public long CreatedEpoch
        //{
        //    get { return this.CreatedDate.ToEpoch(); }
        //    set { this.CreatedDate = Extensions.FromEpoch(value); }
        //}

        public string CreatedDescription { get; private set; }

        public string UpdatedBy { get; private set; }

        public DateTimeOffset UpdatedDate { get; private set; } = DateTimeOffset.UtcNow;

        //public long UpdatedEpoch
        //{
        //    get { return this.UpdatedDate.ToEpoch(); }
        //    set { this.UpdatedDate = Extensions.FromEpoch(value); }
        //}

        public string UpdatedDescription { get; set; }

        public IEnumerable<string> UpdatedReasons { get; private set; }

        public string ExpiredBy { get; set; }

        public DateTimeOffset? ExpiredDate { get; set; }

        //public long? ExpiredEpoch
        //{
        //    get { return this.ExpiredDate.ToEpoch(); }
        //    set { this.ExpiredDate = Extensions.FromEpoch(value); }
        //}

        public string ExpiredDescription { get; set; }

        public bool? Deactivated { get; private set; }

        public IEnumerable<string> DeactivatedReasons { get; private set; }

        public string DeactivatedBy { get; private set; }

        public DateTimeOffset? DeactivatedDate { get; private set; }

        //public long? DeactivatedEpoch
        //{
        //    get { return this.DeactivatedDate.ToEpoch(); }
        //    set { this.DeactivatedDate = Extensions.FromEpoch(value); }
        //}

        public string DeactivatedDescription { get; set; }

        public bool? Deleted { get; private set; }

        public string DeletedBy { get; private set; }

        public DateTimeOffset? DeletedDate { get; private set; }

        //public long? DeletedEpoch
        //{
        //    get { return this.DeletedDate.ToEpoch(); }
        //    set { this.DeletedDate = Extensions.FromEpoch(value); }
        //}

        public string DeletedReason { get; private set; }

        public string DeletedDescription { get; set; }

        public string LastAccessedBy { get; set; }

        public DateTimeOffset? LastAccessedDate { get; set; }

        //public long? LastAccessedEpoch
        //{
        //    get { return this.LastAccessedDate.ToEpoch(); }
        //    set { this.LastAccessedDate = Extensions.FromEpoch(value); }
        //}

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

            if(!by.IsNullOrEmpty())
            {
                this.DeactivatedBy = by;
            }

            if(!reason.IsNullOrEmpty())
            {
                if(this.DeactivatedReasons.IsNullOrEmpty())
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
            if(!by.IsNullOrEmpty())
            {
                this.CreatedBy = by;
            }

            if(!description.IsNullOrEmpty())
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

            if(!by.IsNullOrEmpty())
            {
                this.UpdatedBy = by;
            }

            if(!reason.IsNullOrEmpty())
            {
                if(this.UpdatedReasons.IsNullOrEmpty())
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

            if(!by.IsNullOrEmpty())
            {
                this.DeletedBy = by;
            }

            if(!reason.IsNullOrEmpty())
            {
                this.DeletedReason = $"{by}: ({this.DeletedDate.Value.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim();
            }
        }
    }
}
