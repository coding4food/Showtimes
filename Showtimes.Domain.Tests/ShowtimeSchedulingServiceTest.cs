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
        private IUnitOfWork UnitOfWork;
        private ShowtimesSchedulingService Sut;

        [TestInitialize]
        public void Setup()
        {
            var movies = Mock.Of<IRepository<Movie>>();
            var theatres = Mock.Of<IRepository<MovieTheater>>();
            var showtimes = Mock.Of<IShowtimesRepository>();

            UnitOfWork = Mock.Of<IUnitOfWork>(u =>
                u.MovieTheatres == theatres &&
                u.Movies == movies &&
                u.Showtimes == showtimes
            );

            Sut = new ShowtimesSchedulingService(UnitOfWork);
        }

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

            Mock.Get(UnitOfWork.Movies).Setup(r => r.FindAsync(movie.MovieId)).Returns(Task.FromResult<Movie>(movie));
            Mock.Get(UnitOfWork.MovieTheatres).Setup(r => r.FindAsync(theater.MovieTheaterId)).Returns(Task.FromResult<MovieTheater>(theater));

            await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

            Mock.Get(UnitOfWork.Showtimes).Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                sessionTimes.Contains(s.SessionTime))), Times.Exactly(3));
        }

        [TestMethod]
        public async Task ShowtimesForADate_Returns_Showtimes_For_A_Date()
        {
            var date = DateTime.Today;

            await Sut.ShowtimesForADate(date);

            Mock.Get(UnitOfWork.Showtimes).Verify(r => r.GetAllByDateAsync(date), Times.Once);
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Invalid_MovieId()
        {
            Mock.Get(UnitOfWork.Movies).Setup(r => r.FindAsync(It.IsAny<int>())).Returns(Task.FromResult<Movie>(null));

            Func<Task> act = async () => await Sut.ScheduleShowtimes(1000, 1000, new[] { DateTime.Now });

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("movieId"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Invalid_MovieTheaterId()
        {
            int movieId = 1000;

            Mock.Get(UnitOfWork.Movies).Setup(r => r.FindAsync(movieId))
                .Returns(Task.FromResult<Movie>(new Movie { MovieId = movieId, Title = "AAA" }));
            Mock.Get(UnitOfWork.MovieTheatres).Setup(r => r.FindAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<MovieTheater>(null));

            Func<Task> act = async () => await Sut.ScheduleShowtimes(1000, movieId, new[] { DateTime.Now });

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("movieTheaterId"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Null_SessionTimes()
        {
            int movieId = 1000;
            int movieTheaterId = 1000;

            Mock.Get(UnitOfWork.Movies).Setup(r => r.FindAsync(movieId))
                .Returns(Task.FromResult<Movie>(new Movie { MovieId = movieId, Title = "AAA" }));
            Mock.Get(UnitOfWork.MovieTheatres).Setup(r => r.FindAsync(movieTheaterId))
                .Returns(Task.FromResult<MovieTheater>(new MovieTheater { MovieTheaterId = movieTheaterId, Name = "BBB" }));

            Func<Task> act = async () => await Sut.ScheduleShowtimes(movieTheaterId, movieId, null);

            act.ShouldThrow<ArgumentNullException>().Where(ex => ex.Message.Contains("sessionTimes"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Empty_SessionTimes()
        {
            int movieId = 1000;
            int movieTheaterId = 1000;

            Mock.Get(UnitOfWork.Movies).Setup(r => r.FindAsync(movieId))
                .Returns(Task.FromResult<Movie>(new Movie { MovieId = movieId, Title = "AAA" }));
            Mock.Get(UnitOfWork.MovieTheatres).Setup(r => r.FindAsync(movieTheaterId))
                .Returns(Task.FromResult<MovieTheater>(new MovieTheater { MovieTheaterId = movieTheaterId, Name = "BBB" }));

            Func<Task> act = async () => await Sut.ScheduleShowtimes(movieTheaterId, movieId, Enumerable.Empty<DateTime>());

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("sessionTimes"));
        }
    }
}
