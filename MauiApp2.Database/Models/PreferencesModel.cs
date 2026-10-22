using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class PreferencesModel : IBaseDbEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Preferences { get; set; }
    }
}
