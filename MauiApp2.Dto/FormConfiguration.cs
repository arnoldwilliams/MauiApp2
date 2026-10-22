using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MauiApp2.Dto
{
    public class FormConfiguration
    {
        /// <summary>
        /// Gets or sets the form configuration identifier.
        /// </summary>
        /// <value>
        /// The form configuration identifier.
        /// </value>
        [Required]
        public string FormConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the portal form identifier.
        /// </summary>
        /// <value>
        /// The portal form identifier.
        /// </value>
        [Required]
        public string PortalFormId { get; set; }

        /// <summary>
        /// Gets or sets the form label.
        /// </summary>
        /// <value>
        /// The form label.
        /// </value>
        public string FormLabel { get; set; }

        /// <summary>
        /// Gets or sets the formheader text.
        /// </summary>
        /// <value>
        /// The formheader text.
        /// </value>
        public string FormheaderText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FormConfiguration"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        public DateTime ModifiedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the active date from.
        /// </summary>
        /// <value>
        /// The active date from.
        /// </value>
        public DateTime ActiveDateFromUtc { get; set; }

        /// <summary>
        /// Gets or sets the active date to.
        /// </summary>
        /// <value>
        /// The active date to.
        /// </value>
        public DateTime ActiveDateToUtc { get; set; }

        /// <summary>
        /// Gets or sets the field configurations.
        /// </summary>
        /// <value>
        /// The field configurations.
        /// </value>
        public List<FieldConfiguration> FieldConfigurations { get; set; }
    }
}
