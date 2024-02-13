using AttendanceApp.Model;
using AttendanceApp.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        //Model
        public DbSet<Department> Department { get; set; }
        public DbSet<EmployeeRegister> EmployeeRegister { get; set; }

        //ViewModel
        public DbSet<CustomErrorResponseViewModel> CustomErrorResponseViewModel { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        { 

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomErrorResponseViewModel>().HasNoKey();
        }
    }
}
