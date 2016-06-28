using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showtimes.Domain
{
    public interface IRepository<T> where T: class
    {
        Task<T> FindAsync(params object[] keyValues);
        Task<IEnumerable<T>> GetAllAsync();

        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
    }

    public interface IShowtimesRepository: IRepository<Showtimes>
    {
        Task<IEnumerable<Showtimes>> GetAllByDateAsync(DateTime date);
        Task<IEnumerable<Showtimes>> GetAllByDateAsync(DateTime date, int movieTheaterId, int movieId);
    }

    public interface IUnitOfWork
    {
        IRepository<Movie> Movies { get; }
        IRepository<MovieTheater> MovieTheatres { get; }
        IShowtimesRepository Showtimes { get; }

        Task SaveAsync();
    }

    internal class Repository<T>:  IRepository<T> where T: class
    {
        private DbContext ctx;
        protected DbSet<T> DbSet;

        public Repository(DbContext context)
        {
            this.ctx = context;
            this.DbSet = context.Set<T>();
        }

        public virtual Task<T> FindAsync(params object[] keyValues)
        {
            return this.DbSet.FindAsync(keyValues);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            var data = await this.DbSet.ToArrayAsync();
            return data;
        }

        public virtual void Insert(T entity)
        {
            this.DbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            this.DbSet.Attach(entity);
            this.ctx.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(T entity)
        {
            if (this.ctx.Entry(entity).State == EntityState.Detached)
            {
                this.DbSet.Attach(entity);
            }

            this.DbSet.Remove(entity);
        }
    }

    internal class ShowtimesRepository : Repository<Showtimes>, IShowtimesRepository
    {
        public ShowtimesRepository(DbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Showtimes>> GetAllByDateAsync(DateTime date)
        {
            var endDate = date.Date.AddDays(1);

            var showtimes = await this.DbSet
                .Where(s => s.SessionTime >= date.Date && s.SessionTime < endDate)
                .ToArrayAsync();

            return showtimes;
        }

        public async Task<IEnumerable<Showtimes>> GetAllByDateAsync(DateTime date, int movieTheaterId, int movieId)
        {
            var endDate = date.Date.AddDays(1);

            var showtimes = await this.DbSet
                .Where(s => s.MovieTheaterId == movieTheaterId && s.MovieId == movieId && s.SessionTime >= date.Date && s.SessionTime < endDate)
                .ToArrayAsync();

            return showtimes;
        }
    }

    public class UnitOfWork: IUnitOfWork, IDisposable
    {
        private DbContext ctx;
        private IRepository<Movie> movies;
        private IRepository<MovieTheater> movieTheatres;
        private IShowtimesRepository showtimes;

        public IRepository<Movie> Movies
        {
            get
            {
                if (this.movies == null)
                {
                    this.movies = new Repository<Movie>(this.ctx);
                }

                return this.movies;
            }
        }

        public IRepository<MovieTheater> MovieTheatres
        {
            get
            {
                if (this.movieTheatres == null)
                {
                    this.movieTheatres = new Repository<MovieTheater>(this.ctx);
                }

                return this.movieTheatres;
            }
        }

        public IShowtimesRepository Showtimes
        {
            get
            {
                if (this.showtimes == null)
                {
                    this.showtimes = new ShowtimesRepository(this.ctx);
                }

                return this.showtimes;
            }
        }

        public UnitOfWork(): this(new ShowtimesContext()) { }

        public UnitOfWork(DbContext context)
        {
            this.ctx = context;
        }

        public Task SaveAsync() => this.ctx.SaveChangesAsync();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.ctx.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UnitOfWork() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
