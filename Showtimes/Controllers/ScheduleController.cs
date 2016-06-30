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
            // TODO ??? add validation for movie and theater (should exist)

            var d = date ?? DateTime.Today;

            var showtimes = await unitOfWork.Showtimes.GetAllByDateAsync(d, movieTheaterId, movieId);
            var theatres = await unitOfWork.MovieTheatres.GetAllAsync();
            var movies = await unitOfWork.Movies.GetAllAsync();

            ViewBag.Movies = new SelectList(
                items: movies,
                dataTextField: "Title",
                dataValueField: "MovieId",
                selectedValue: movieId
            );

            ViewBag.MovieTheaters = new SelectList(
                items: theatres,
                dataTextField: "Name",
                dataValueField: "MovieTheaterId",
                selectedValue: movieTheaterId
            );

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
        public ActionResult Edit([Bind(Include = "MovieTheaterId, MovieId, Date, SessionTimesStr")] ShowtimesEdit model)
        {
            try
            {
                // TODO: Add update logic here
                if (ModelState.IsValid)
                {
                    return RedirectToAction("Index", new { date = model.Date });
                }

                return View(model);
            }
            catch
            {
                return View();
            }
        }

        // GET: Schedule/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Schedule/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete([Bind(Include = "MovieTheaterId, MovieId, Date, SessionTimesStr")] ShowtimesEdit model)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index", new { date = model.Date });
            }
            catch
            {
                return View();
            }
        }
    }
}
