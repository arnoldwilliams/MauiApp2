using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Database.Models
{
    public class CaseModel : IBaseDbEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public bool? Posted { get; set; }

        public string CaseNumber { get; set; }

        public string CaseType { get; set; }

        public string CaseDTOJson { get; set; }

        public string Status { get; set; }

        public string CommentsNotes { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CaseDTOVerson { get; set; }

        //public int Retries { get; set; }

        //public DateTime LastPostAttempt { get; set; }
    }
}
