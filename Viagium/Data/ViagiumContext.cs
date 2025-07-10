using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Viagium.Models;

namespace Viagium.Data
{
    public class ViagiumContext : DbContext
    {
        public ViagiumContext (DbContextOptions<ViagiumContext> options)
            : base(options)
        {
        }

        public DbSet<Viagium.Models.Employees> Employees { get; set; } = default!;
    }
}
