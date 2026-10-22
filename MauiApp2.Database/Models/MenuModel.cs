using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp2.Database.Models
{
    public class MenuModel
    {
        [PrimaryKey]
        public string MenuId { get; set; }

        public string MenuJSON { get; set; }
    }
}
