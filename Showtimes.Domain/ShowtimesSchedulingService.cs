using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showtimes.Domain
{
    public static class ShowtimesSchedulingService
    {
        public static Task ScheduleShowtimes(int movieTheaterId, int movieId, IEnumerable<DateTime> sessionTimes)
        {
            // TODO add some validation

            // HIDDEN DEPENDENCY
            var uow = new UnitOfWork();

            foreach (var showtime in sessionTimes.Select(time => new Showtimes(movieTheaterId, movieId, time)))
            {
                uow.Showtimes.Insert(showtime);
            }

            return uow.SaveAsync();
        }

        public static Task<IEnumerable<Showtimes>> ShowtimesForADate(DateTime date)
        {
            // Requires Dependency Injection
            var uow = new UnitOfWork();

            return uow.Showtimes.GetAllByDateAsync(date);
        }
    }
}
