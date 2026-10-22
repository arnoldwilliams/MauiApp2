using MauiApp2.Database.Models.Abstractions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class TokenStorageModel : IBaseDbEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string IDToken { get; set; }

        public string AccessToken { get; set; }

        public string UserDisplayName { get; set; }

        public string UserId { get; set; }
    }
}
