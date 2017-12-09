using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Paster.Models
{
    public class PasteContext : DbContext
    {
        public PasteContext() : base("PasteContext")
        {
        }

        public DbSet<Pastes> Pastes { get; set; }
    }
}