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
        private Movie movie = new Movie { MovieId = 10, Title = "asdfgh" };
        private MovieTheater theater = new MovieTheater { MovieTheaterId = 15, Name = "qwerty" };

        [TestInitialize]
        public void Setup()
        {
            var movies = Mock.Of<IRepository<Movie>>(r => r.FindAsync(movie.MovieId) == Task.FromResult(movie));
            var theatres = Mock.Of<IRepository<MovieTheater>>(r => r.FindAsync(theater.MovieTheaterId) == Task.FromResult(theater));
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
            var date = DateTime.Today;
            var sessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(14),
            };

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
            Mock.Get(UnitOfWork.MovieTheatres).Setup(r => r.FindAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<MovieTheater>(null));

            Func<Task> act = async () => await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, new[] { DateTime.Now });

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("movieTheaterId"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Null_SessionTimes()
        {
            Func<Task> act = async () => await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, null);

            act.ShouldThrow<ArgumentNullException>().Where(ex => ex.Message.Contains("sessionTimes"));
        }

        [TestMethod]
        public void ScheduleShowtime_Throws_For_Empty_SessionTimes()
        {
            Func<Task> act = async () => await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, Enumerable.Empty<DateTime>());

            act.ShouldThrow<ArgumentException>().Where(ex => ex.Message.Contains("sessionTimes"));
        }

        [TestMethod]
        public async Task ScheduleShowtime_Ignores_Duplicate_Session_Times()
        {
            var date = DateTime.Today;
            var sessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(14)
            };

            await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, sessionTimes);

            var mock = Mock.Get(UnitOfWork.Showtimes);

            mock.Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == sessionTimes[0])), Times.Once());

            mock.Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == sessionTimes[1])), Times.Once());

            mock.Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == sessionTimes[4])), Times.Once());
        }

        [TestMethod]
        public async Task ScheduleShowtime_Removes_Missing_Showtimes_For_A_Date()
        {
            var date = DateTime.Today;
            var oldSessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(14)
            };
            var newSessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(13),
                date.AddHours(15)
            };

            Mock.Get(UnitOfWork.Showtimes).Setup(r => r.GetAllByDateAsync(date, theater.MovieTheaterId, movie.MovieId))
                .Returns(Task.FromResult(oldSessionTimes.Select(t => new Showtimes(theater.MovieTheaterId, movie.MovieId, t))));

            await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, newSessionTimes);

            var mock = Mock.Get(UnitOfWork.Showtimes);

            mock.Verify(r => r.Delete(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == oldSessionTimes[1])), Times.Once());

            mock.Verify(r => r.Delete(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == oldSessionTimes[2])), Times.Once());
        }

        [TestMethod]
        public async Task ScheduleShowtime_Inserts_New_Showtimes_For_A_Date()
        {
            var date = DateTime.Today;
            var oldSessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(12),
                date.AddHours(14)
            };
            var newSessionTimes = new[]
            {
                date.AddHours(10),
                date.AddHours(13),
                date.AddHours(15)
            };

            Mock.Get(UnitOfWork.Showtimes).Setup(r => r.GetAllByDateAsync(date, theater.MovieTheaterId, movie.MovieId))
                .Returns(Task.FromResult(oldSessionTimes.Select(t => new Showtimes(theater.MovieTheaterId, movie.MovieId, t))));

            await Sut.ScheduleShowtimes(theater.MovieTheaterId, movie.MovieId, newSessionTimes);

            var mock = Mock.Get(UnitOfWork.Showtimes);

            mock.Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == newSessionTimes[1])), Times.Once());

            mock.Verify(r => r.Insert(It.Is<Showtimes>(s =>
                s.MovieTheaterId == theater.MovieTheaterId &&
                s.MovieId == movie.MovieId &&
                s.SessionTime == newSessionTimes[2])), Times.Once());
        }
    }
}
