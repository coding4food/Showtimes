using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showtimes.Domain
{
    public static class ShowtimesSchedulingService
    {
        public static void ScheduleShowtimes(int movieTheaterId, int movieId, IEnumerable<DateTime> sessionTimes)
        {
            // TODO add some validation
        }

        public static IEnumerable<Showtimes> ShowtimesForADate(DateTime date)
        {
            return Enumerable.Empty<Showtimes>();
        }
    }
}
