using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showtimes.Domain
{
    public class ShowtimesSchedulingService
    {
        private IUnitOfWork unitOfWork;

        public ShowtimesSchedulingService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task ScheduleShowtimes(int movieTheaterId, int movieId, IEnumerable<DateTime> sessionTimes)
        {
            // TODO add some validation

            if (sessionTimes == null)
            {
                throw new ArgumentNullException(nameof(sessionTimes));
            }

            if (!sessionTimes.Any())
            {
                // тут ArgumentException, потому что при таком списке сеансов невозможно получить дату расписания
                // если бы дата и сеансы передавались отдельно, можно было бы удалить все сеансы при пустом списке на этот день
                throw new ArgumentException(nameof(sessionTimes));
            }

            if ((await this.unitOfWork.Movies.FindAsync(movieId)) == null)
            {
                throw new ArgumentException(nameof(movieId));
            }

            if ((await this.unitOfWork.MovieTheatres.FindAsync(movieTheaterId)) == null)
            {
                throw new ArgumentException(nameof(movieTheaterId));
            }

            foreach (var showtime in sessionTimes.Distinct().Select(time => new Showtimes(movieTheaterId, movieId, time)))
            {
                this.unitOfWork.Showtimes.Insert(showtime);
            }

            await this.unitOfWork.SaveAsync();
        }

        public Task<IEnumerable<Showtimes>> ShowtimesForADate(DateTime date)
        {
            return this.unitOfWork.Showtimes.GetAllByDateAsync(date);
        }
    }
}
