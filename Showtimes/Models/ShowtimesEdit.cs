using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Showtimes.Models
{
    public class ShowtimesEdit
    {
        [Required]
        public int MovieTheaterId { get; set; }
        [Required]
        public int MovieId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string SessionTimesStr { get; set; }
    }
}