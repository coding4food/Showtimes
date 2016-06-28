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
            var movie = new Movie { MovieId = 10, Title = "asdfgh" };
            var theater = new MovieTheater { MovieTheaterId = 15, Name = "qwerty" };
            var date = DateTime.Today;
            var sessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(14),
            };

            var mockMovies = Mock.Of<IRepository<Movie>>(r => r.FindAsync(movie.MovieId) == Task.FromResult<Movie>(movie));
            var mockTheatres = Mock.Of<IRepository<MovieTheater>>(r => r.FindAsync(theater.MovieTheaterId) == Task.FromResult<MovieTheater>(theater));
            var mockShowtimes = Mock.Of<IShowtimesRepository>();
            var uow = Mock.Of<IUnitOfWork>(u => u.Movies == mockMovies && u.MovieTheatres == mockTheatres && u.Showtimes == mockShowtimes);

            var sut = new ShowtimesSchedulingService(uow);

            await sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

            Mock.Get(mockShowtimes).Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                sessionTimes.Contains(s.SessionTime))), Times.Exactly(3));
        }

        [TestMethod]
        public async Task ShowtimesForADate_Returns_Showtimes_For_A_Date()
        {
            var date = DateTime.Today;

            var mockShowtimes = Mock.Of<IShowtimesRepository>();
            var uow = Mock.Of<IUnitOfWork>(u => u.Showtimes == mockShowtimes);

            var sut = new ShowtimesSchedulingService(uow);

            await sut.ShowtimesForADate(date);

            Mock.Get(mockShowtimes).Verify(r => r.GetAllByDateAsync(date), Times.Once);
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
