using Microsoft.EntityFrameworkCore;
using Seisundid.Models;

namespace Seisundid.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Pood> Poed { get; set; }
        public DbSet<PaevaGraafik> PaevaGraafiks { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}