using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Showtimes.Domain.Tests
{
    [TestClass]
    public class ShowtimeSchedulingServiceTest
    {
        [TestMethod]
        public async Task ScheduleShowtime_Stores_Showtimes()
        {
            var uow = new UnitOfWork();

            uow.MovieTheatres.Insert(new MovieTheater { Name = "qwerty" });
            uow.Movies.Insert(new Movie { Title = "asdfgh" });

            await uow.SaveAsync();

            var theater = (await uow.MovieTheatres.GetAllAsync()).First();
            var movie = (await uow.Movies.GetAllAsync()).First();
            var sessionTimes = new[]
            {
                DateTime.Today.AddHours(10),
                DateTime.Today.AddHours(12),
                DateTime.Today.AddHours(14),
            };

            await ShowtimesSchedulingService.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

            Assert.AreEqual(sessionTimes.Length, (await uow.Showtimes.GetAllAsync()).Count());
        }

        [TestMethod]
        public async Task ShowtimesForADate_Returns_Showtimes_For_A_Date()
        {
            var date = DateTime.Today;

            var sessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(14),
            };

            var uow = new UnitOfWork();

            uow.MovieTheatres.Insert(new MovieTheater { Name = "qwerty" });
            uow.Movies.Insert(new Movie { Title = "asdfgh" });

            await uow.SaveAsync();

            var theater = (await uow.MovieTheatres.GetAllAsync()).First();
            var movie = (await uow.Movies.GetAllAsync()).First();

            await ShowtimesSchedulingService.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

            var actual = await ShowtimesSchedulingService.ShowtimesForADate(date);

            Assert.IsTrue(actual.Any());
            Assert.AreEqual(sessionTimes.Length, actual.Count());
            CollectionAssert.AreEquivalent(sessionTimes, actual.Select(s => s.SessionTime).ToArray());
        }
    }
}
