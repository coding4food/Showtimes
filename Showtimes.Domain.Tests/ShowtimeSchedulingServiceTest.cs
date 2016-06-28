using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

            await (new ShowtimesSchedulingService(uow)).ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

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

            var sut = new ShowtimesSchedulingService(uow);

            await sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

            var actual = await sut.ShowtimesForADate(date);

            Assert.IsTrue(actual.Any());
            Assert.AreEqual(sessionTimes.Length, actual.Count());
            CollectionAssert.AreEquivalent(sessionTimes, actual.Select(s => s.SessionTime).ToArray());
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Invalid_MovieId()
        {
            var mockMovies = Mock.Of<IRepository<Movie>>(r => r.FindAsync(It.IsAny<int>()) == Task.FromResult<Movie>(null));
            var uow = Mock.Of<IUnitOfWork>(u => u.Movies == mockMovies);

            var sut = new ShowtimesSchedulingService(uow);

            Func<Task> act = async () => await sut.ScheduleShowtimes(1000, 1000, new[] { DateTime.Now });

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("movieId"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Invalid_MovieTheaterId()
        {
            int movieId = 1000;

            var mockMovies = Mock.Of<IRepository<Movie>>(r => r.FindAsync(movieId) == Task.FromResult<Movie>(new Movie { MovieId = movieId, Title = "AAA" }));
            var mockTheatres = Mock.Of<IRepository<MovieTheater>>(r => r.FindAsync(It.IsAny<int>()) == Task.FromResult<MovieTheater>(null));
            var uow = Mock.Of<IUnitOfWork>(u => u.Movies == mockMovies && u.MovieTheatres == mockTheatres);

            var sut = new ShowtimesSchedulingService(uow);

            Func<Task> act = async () => await sut.ScheduleShowtimes(1000, movieId, new[] { DateTime.Now });

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("movieTheaterId"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Null_SessionTimes()
        {
            int movieId = 1000;
            int movieTheaterId = 1000;

            var mockMovies = Mock.Of<IRepository<Movie>>(r => r.FindAsync(movieId) == Task.FromResult<Movie>(new Movie { MovieId = movieId, Title = "AAA" }));
            var mockTheatres = Mock.Of<IRepository<MovieTheater>>(r => r.FindAsync(movieTheaterId) == Task.FromResult<MovieTheater>(new MovieTheater { MovieTheaterId = movieTheaterId, Name = "BBB" }));
            var uow = Mock.Of<IUnitOfWork>(u => u.Movies == mockMovies && u.MovieTheatres == mockTheatres);

            var sut = new ShowtimesSchedulingService(uow);

            Func<Task> act = async () => await sut.ScheduleShowtimes(movieTheaterId, movieId, null);

            act.ShouldThrow<ArgumentNullException>().Where(ex => ex.Message.Contains("sessionTimes"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Empty_SessionTimes()
        {
            int movieId = 1000;
            int movieTheaterId = 1000;

            var mockMovies = Mock.Of<IRepository<Movie>>(r => r.FindAsync(movieId) == Task.FromResult<Movie>(new Movie { MovieId = movieId, Title = "AAA" }));
            var mockTheatres = Mock.Of<IRepository<MovieTheater>>(r => r.FindAsync(movieTheaterId) == Task.FromResult<MovieTheater>(new MovieTheater { MovieTheaterId = movieTheaterId, Name = "BBB" }));
            var uow = Mock.Of<IUnitOfWork>(u => u.Movies == mockMovies && u.MovieTheatres == mockTheatres);

            var sut = new ShowtimesSchedulingService(uow);

            Func<Task> act = async () => await sut.ScheduleShowtimes(movieTheaterId, movieId, Enumerable.Empty<DateTime>());

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("sessionTimes"));
        }
    }
}
