using GestionAcademica.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.API.Data
{
    public class GestionAcademicaDbContext : DbContext
    {
        public GestionAcademicaDbContext(DbContextOptions<GestionAcademicaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Asignacion> Asignaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Curso>(e =>
            {
                e.ToTable("Cursos");
                e.HasKey(c => c.CursoId);
                e.Property(c => c.CursoId).ValueGeneratedOnAdd();
                e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Estudiante>(e =>
            {
                e.ToTable("Estudiantes");
                e.HasKey(x => x.EstudianteId);
                e.Property(x => x.EstudianteId).ValueGeneratedOnAdd();
                e.Property(x => x.Nombres).IsRequired().HasMaxLength(100);
                e.Property(x => x.Apellidos).IsRequired().HasMaxLength(100);
                e.Property(x => x.Direccion).HasMaxLength(200);
            });

            modelBuilder.Entity<Docente>(e =>
            {
                e.ToTable("Docentes");
                e.HasKey(x => x.DocenteId);
                e.Property(x => x.DocenteId).ValueGeneratedOnAdd();
                e.Property(x => x.Nombres).IsRequired().HasMaxLength(100);
                e.Property(x => x.Apellidos).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Asignacion>(e =>
            {
                e.ToTable("Asignaciones");
                e.HasKey(a => a.AsignacionId);
                e.Property(a => a.AsignacionId).ValueGeneratedOnAdd();

                e.HasOne(a => a.Estudiante)
                    .WithMany(es => es.Asignaciones)
                    .HasForeignKey(a => a.EstudianteId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Curso)
                    .WithMany(c => c.Asignaciones)
                    .HasForeignKey(a => a.CursoId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Docente)
                    .WithMany(d => d.Asignaciones)
                    .HasForeignKey(a => a.DocenteId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
