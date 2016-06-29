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
                MovieTheatres = await unitOfWork.MovieTheatres.GetAllAsync(),
                Movies = await unitOfWork.Movies.GetAllAsync(),
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
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
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
