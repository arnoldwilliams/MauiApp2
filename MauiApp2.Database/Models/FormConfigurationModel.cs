using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class FormConfigurationModel : IBaseDbEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string FormID { get; set; }

        public string Json { get; set; }
    }
}
