using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class CaseSummaryModel : IBaseDbEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string DraftId { get; set; }
        public string CaseNumber { get; set; }
        public string CaseType { get; set; }
        public string CaseTypeDisplayName { get; set; }
        public string CaseDtoJson { get; set; }
        public int CaseDtoVersion { get; set; }
        public bool IsLocalDraft { get; set; }
        public string ExternalDetailsUrl { get; set; }
        public DateTime TimestampUtc { get; set; }
        public DateTime CreatedDate { get; set; }
        public string State { get; set; }
        public DateTime StatusUpdateTimestampUtc { get; set; }
        public double CoordEastings { get; set; }
        public double CoordNorthings { get; set; }
    }

}
