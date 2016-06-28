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

        public Task ScheduleShowtimes(int movieTheaterId, int movieId, IEnumerable<DateTime> sessionTimes)
        {
            // TODO add some validation

            foreach (var showtime in sessionTimes.Select(time => new Showtimes(movieTheaterId, movieId, time)))
            {
                this.unitOfWork.Showtimes.Insert(showtime);
            }

            return this.unitOfWork.SaveAsync();
        }

        public Task<IEnumerable<Showtimes>> ShowtimesForADate(DateTime date)
        {
            return this.unitOfWork.Showtimes.GetAllByDateAsync(date);
        }
    }
}
