using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class BannedExtensionListModel : IBaseDbEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string FileExtension { get; set; }
    }
}
