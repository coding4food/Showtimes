using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Showtimes.Domain
{
    public class ShowtimesContext : DbContext
    {
        public virtual DbSet<MovieTheater> MovieTheatres { get; set; }
        public virtual DbSet<Movie> Movies { get; set; }
        public virtual DbSet<Showtimes> Showtimes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>().Property(m => m.Title).IsRequired();

            modelBuilder.Entity<MovieTheater>().Property(t => t.Name).IsRequired();

            modelBuilder.Entity<Showtimes>().HasRequired(s => s.Theater).WithMany().HasForeignKey(_ => _.MovieTheaterId);

            modelBuilder.Entity<Showtimes>().HasRequired(s => s.Movie).WithMany().HasForeignKey(_ => _.MovieId);

            modelBuilder.Entity<Showtimes>().HasKey(s => new { s.MovieTheaterId, s.MovieId, s.SessionTime });
        }
    }

    public class ScheduleInitializer: CreateDatabaseIfNotExists<ShowtimesContext>
    {
        protected override void Seed(ShowtimesContext context)
        {
            base.Seed(context);

            DatabaseSeeder.Seed(context);
        }
    }

    public static class DatabaseSeeder
    {
        public static void Seed(ShowtimesContext ctx)
        {
            ctx.Movies.AddRange(new[] {
                new Movie { Title = "Побег из Шоушенка" },
                new Movie { Title = "Зеленая миля" },
                new Movie { Title = "Форрест Гамп" },
                new Movie { Title = "Список Шиндлера" },
                new Movie { Title = "1+1" },
                new Movie { Title = "Начало" },
                new Movie { Title = "Король Лев" },
                new Movie { Title = "Леон" },
                new Movie { Title = "Бойцовский клуб" },
                new Movie { Title = "Жизнь прекрасна" },
                new Movie { Title = "Иван Васильевич меняет профессию" },
                new Movie { Title = "Достучаться до небес" },
                new Movie { Title = "Крестный отец" },
                new Movie { Title = "Интерстеллар" },
                new Movie { Title = "Престиж" },
                new Movie { Title = "Игры разума" },
                new Movie { Title = "Криминальное чтиво" },
                new Movie { Title = "Операция «Ы» и другие приключения Шурика" },
                new Movie { Title = "Властелин колец: Возвращение Короля" },
                new Movie { Title = "Гладиатор" },
            });

            ctx.MovieTheatres.AddRange(new[] {
                new MovieTheater { Name = "Маяковский" },
                new MovieTheater { Name = "Галактика" },
                new MovieTheater { Name = "Слава" },
                new MovieTheater { Name = "Вавилон" },
                new MovieTheater { Name = "Кристалл" },
            });

            ctx.SaveChanges();

            var date = new DateTime(2016, 6, 1);

            foreach (var t in ctx.MovieTheatres)
            {
                foreach (var m in ctx.Movies.Take(5))
                {
                    for (int i = 1; i < 5; i++)
                    {
                        var d = date.AddDays(i);

                        foreach (var s in new[] { d.AddHours(10), d.AddHours(12), d.AddHours(14) })
                        {
                            ctx.Showtimes.Add(new Showtimes(t.MovieTheaterId, m.MovieId, s));
                        }
                    }
                }
            }

            ctx.SaveChanges();
        }
    }
}