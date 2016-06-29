using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Showtimes.Domain;

namespace Showtimes.Models
{
    public class ShowtimesList
    {
        [DataType(DataType.Date)]
        [UIHint("Date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today;
        public IEnumerable<MovieTheater> MovieTheatres { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
        public IEnumerable<Showtimes.Domain.Showtimes> Showtimes { get; set; }
    }
}