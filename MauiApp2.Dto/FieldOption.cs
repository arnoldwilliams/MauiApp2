using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MauiApp2.Dto
{
    public class FieldOption
    {
        /// <summary>
        /// Gets or sets the field option identifier.
        /// </summary>
        /// <value>
        /// The field option identifier.
        /// </value>
        [Required]
        public string FieldOptionId { get; set; }

        /// <summary>
        /// Gets or sets the option value.
        /// </summary>
        /// <value>
        /// The option value.
        /// </value>
        [Required]
        public string OptionValue { get; set; }

        /// <summary>
        /// Gets or sets the option label.
        /// </summary>
        /// <value>
        /// The option label.
        /// </value>
        [Required]
        public string OptionLabel { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the dependent field to show.
        /// </summary>
        /// <value>
        /// The dependent field to show.
        /// </value>
        public List<string> DependentFields { get; set; }
    }
}
