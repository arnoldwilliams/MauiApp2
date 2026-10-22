using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class LogModel : IBaseDbEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Log { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }
}
