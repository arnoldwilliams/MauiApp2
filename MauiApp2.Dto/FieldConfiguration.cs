using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MauiApp2.Dto
{
    public class FieldConfiguration
    {
        /// <summary>
        /// Gets or sets the field configuration identifier.
        /// </summary>
        /// <value>
        /// The field configuration identifier.
        /// </value>
        [Required]
        public string FieldConfigurationId { get; set; }

        //public string FormConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the portal field identifier.
        /// </summary>
        /// <value>
        /// The portal field identifier.
        /// </value>
        [Required]
        public string PortalFieldId { get; set; }

        /// <summary>
        /// Gets or sets the field label.
        /// </summary>
        /// <value>
        /// The field label.
        /// </value>
        public string FieldLabel { get; set; }

        /// <summary>
        /// Gets or sets the field descriptive text.
        /// </summary>
        /// <value>
        /// The field descriptive text.
        /// </value>
        public string FieldDescriptiveText { get; set; }

        /// <summary>
        /// Gets or sets the field important text.
        /// </summary>
        /// <value>
        /// The field important text.
        /// </value>
        public string FieldImportantText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [field mandatory].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [field mandatory]; otherwise, <c>false</c>.
        /// </value>
        public bool FieldMandatory { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>
        /// The maximum length.
        /// </value>
        public int MaximumLength { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the option set.
        /// </summary>
        /// <value>
        /// The option set.
        /// </value>
        public string OptionSet { get; set; }

        /// <summary>
        /// Gets or sets the field options.
        /// </summary>
        /// <value>
        /// The field options.
        /// </value>
        public List<FieldOption> FieldOptions { get; set; }
    }
}
