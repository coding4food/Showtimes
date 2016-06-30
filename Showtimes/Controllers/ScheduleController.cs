using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Showtimes.Domain;
using Showtimes.Models;
using System.Threading.Tasks;

namespace Showtimes.Controllers
{
    public class ScheduleController : Controller
    {
        private IUnitOfWork unitOfWork = new UnitOfWork();

        // GET: Schedule
        public async Task<ActionResult> Index(DateTime? date)
        {
            var d = date ?? DateTime.Today.AddSeconds(1);

            var model = new ShowtimesList
            {
                Date = d,
                Showtimes = await unitOfWork.Showtimes.GetAllByDateAsync(d)
            };

            return View(model);
        }

        // GET: Schedule/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Schedule/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Schedule/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Schedule/Edit/5
        public async Task<ActionResult> Edit(int movieTheaterId, int movieId, DateTime? date)
        {
            var d = date ?? DateTime.Today;

            var showtimes = await unitOfWork.Showtimes.GetAllByDateAsync(d, movieTheaterId, movieId);

            await FillMoviesSelectList(movieId);
            await FillMovieTheatresSelectList(movieTheaterId);

            var model = new ShowtimesEdit
            {
                MovieTheaterId = movieTheaterId,
                MovieId = movieId,
                Date = d,
                SessionTimesStr = string.Join("\n", showtimes.Select(_ => _.SessionTime.TimeOfDay.ToString("hh\\:mm")))
            };

            return View(model);
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MovieTheaterId, MovieId, Date, SessionTimesStr")] ShowtimesEdit model)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(model.SessionTimesStr))
                {
                    TimeSpan ts;

                    var sessionTimesInput = model.SessionTimesStr
                        .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim());

                    var sessionTimesAreInCorrectFormat = sessionTimesInput
                        .All(s => TimeSpan.TryParseExact(s, new[] { "h\\:mm", "hh\\:mm" }, null, out ts));

                    if (!sessionTimesAreInCorrectFormat)
                    {
                        ModelState.AddModelError("SessionTimesStr", "Session Times are invalid");
                    }
                    else if (sessionTimesInput.Distinct().Count() != sessionTimesInput.Count())
                    {
                        ModelState.AddModelError("SessionTimesStr", "Duplicate session times");
                    }
                }

                if ((await unitOfWork.MovieTheatres.FindAsync(model.MovieTheaterId)) == null)
                {
                    ModelState.AddModelError("MovieTheaterId", "Movie theater doesn't exist");
                }

                if ((await unitOfWork.Movies.FindAsync(model.MovieId)) == null)
                {
                    ModelState.AddModelError("MovieId", "Movie doesn't exist");
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction("Index", new { date = model.Date });
                }

                await FillMoviesSelectList(model.MovieId);
                await FillMovieTheatresSelectList(model.MovieTheaterId);

                return View(model);
            }
            catch
            {
                return View();
            }
        }

        private async Task FillMovieTheatresSelectList(int movieTheaterId)
        {
            var theatres = await unitOfWork.MovieTheatres.GetAllAsync();

            ViewBag.MovieTheaters = new SelectList(
                items: theatres,
                dataTextField: "Name",
                dataValueField: "MovieTheaterId",
                selectedValue: movieTheaterId
            );
        }

        private async Task FillMoviesSelectList(int movieId)
        {
            var movies = await unitOfWork.Movies.GetAllAsync();

            ViewBag.Movies = new SelectList(
                items: movies,
                dataTextField: "Title",
                dataValueField: "MovieId",
                selectedValue: movieId
            );
        }

        // POST: Schedule/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int movieTheaterId, int movieId, DateTime date)
        {
            // Удаление будет очень простое: удаляем из БД сеансы для заданных кинотеатра, фильма и даты и редиректим на главную.
            // В случае ошибки тоже редиректим, в списке будет видно, удалилось расписание или нет.
            try
            {
                await unitOfWork.Showtimes.DeleteAllByDate(movieTheaterId, movieId, date);
                await unitOfWork.SaveAsync();

                return RedirectToAction("Index", new { date = date });
            }
            catch
            {
                return RedirectToAction("Index", new { date = date });
            }
        }
    }
}
