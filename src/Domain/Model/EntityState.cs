namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Naos.Core.Common;

    public class EntityState
    {
        public string IdentifierHash { get; private set; }

        public string CreatedBy { get; set; }

        public DateTimeEpoch CreatedDate { get; set; } = new DateTimeEpoch();

        public string CreatedDescription { get; set; }

        public string UpdatedBy { get; set; }

        public DateTimeEpoch UpdatedDate { get; set; } = new DateTimeEpoch();

        public string UpdatedDescription { get; set; }

        public IEnumerable<string> UpdatedReasons { get; set; }

        public string ExpiredBy { get; set; }

        public DateTimeEpoch ExpiredDate { get; set; }

        public string ExpiredDescription { get; set; }

        public bool? Deactivated { get; set; }

        public IEnumerable<string> DeactivatedReasons { get; set; }

        public string DeactivatedBy { get; set; }

        public DateTimeEpoch DeactivatedDate { get; set; }

        public string DeactivatedDescription { get; set; }

        public bool? Deleted { get; set; }

        public string DeletedBy { get; set; }

        public DateTimeEpoch DeletedDate { get; set; }

        public string DeletedReason { get; set; }

        public string DeletedDescription { get; set; }

        public string LastAccessedBy { get; set; }

        public DateTimeEpoch LastAccessedDate { get; set; }

        public string LastAccessedDescription { get; set; }

        public DateTimeEpoch LastActionDate
        {
            get
            {
                return new List<DateTimeEpoch> { this.CreatedDate, this.UpdatedDate, this.DeletedDate/*, this.LastAccessedDate*/ }
                    .Where(d => d != null).NullToEmpty().Max();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether determines whether this instance is active.
        /// </summary>
        public virtual bool IsDeactivated
        {
            get
            {
                return (this.Deactivated != null && (bool)this.Deactivated) || !this.DeactivatedReasons.IsNullOrEmpty();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether determines whether this instance is deleted.
        /// </summary>
        public virtual bool IsDeleted
        {
            get { return (this.Deleted != null && (bool)this.Deleted) || !this.DeletedReason.IsNullOrEmpty(); }
        }

        /// <summary>
        /// Updates the version identifier to the current instance state
        /// </summary>
        /// <param name="entity">The entity to calculate the hash for.</param>
        public virtual void UpdateIdentifierHash(IEntity entity)
        {
            // TODO: omit .State from the hashcode generation
            this.IdentifierHash = HashAlgorithm.ComputeHash(entity);
        }

        /// <summary>
        ///     Sets the deactivated information.
        /// </summary>
        /// <param name="by">Name of the deactivator.</param>
        /// <param name="reason">The reason.</param>
        public virtual void SetDeactivated(string by = null, string reason = null)
        {
            this.DeactivatedDate = new DateTimeEpoch();
            this.SetUpdated(by);
            this.Deactivated = true;
            this.DeactivatedBy = by;
            if (this.DeactivatedReasons.IsNullOrEmpty())
            {
                this.DeactivatedReasons = new List<string>();
            }

            if (by.IsNullOrEmpty() && reason.IsNullOrEmpty())
            {
                return;
            }

            this.DeactivatedReasons = this.DeactivatedReasons.Concat(new[]
                {
                    $"{by}: ({this.DeactivatedDate.DateTime.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim()
                });
        }

        /// <summary>
        /// Sets the created information.
        /// </summary>
        /// <param name="by">Name of the account of the creater.</param>
        /// <param name="description">The description for the creation.</param>
        public virtual void SetCreated(string by = null, string description = null)
        {
            this.CreatedDate = new DateTimeEpoch();
            this.CreatedBy = by;
            this.CreatedDescription = description;
        }

        /// <summary>
        ///     Sets the updated information.
        /// </summary>
        /// <param name="by">Name of the account of the updater.</param>
        /// <param name="reason">The reason of the update.</param>
        public virtual void SetUpdated(string by = null, string reason = null)
        {
            this.UpdatedDate = new DateTimeEpoch();
            this.UpdatedBy = by;

            if (by.IsNullOrEmpty() && reason.IsNullOrEmpty())
            {
                return;
            }

            if (this.UpdatedReasons.IsNullOrEmpty())
            {
                this.UpdatedReasons = new List<string>();
            }

            this.UpdatedReasons = this.UpdatedReasons.Concat(new[]
            {
                $"{by}: ({this.UpdatedDate.DateTime.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim()
            });
        }

        /// <summary>
        ///     Sets the deleted information.
        /// </summary>
        /// <param name="by">Name of the deleter.</param>
        /// <param name="reason">The reason.</param>
        public virtual void SetDeleted(string by = null, string reason = null)
        {
            this.Deleted = true;
            this.DeletedDate = new DateTimeEpoch();
            this.UpdatedDate = this.DeletedDate;
            this.DeletedBy = by;

            if (by.IsNullOrEmpty() && reason.IsNullOrEmpty())
            {
                return;
            }

            this.DeletedReason = $"{by}: ({this.DeletedDate.DateTime.ToString(CultureInfo.InvariantCulture)}) {reason}".Trim();
        }
    }
}
