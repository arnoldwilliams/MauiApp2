using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Database.Models.Abstractions
{
    public interface IBaseDbEntity
    {
        int Id { get; set; }
    }
}
