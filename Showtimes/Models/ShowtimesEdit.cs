using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Showtimes.Models
{
    public class ShowtimesEdit
    {
        public int MovieTheaterId { get; set; }
        public int MovieId { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public IEnumerable<TimeSpan> SessionTimes { get; set; }
        [DataType(DataType.MultilineText)]
        public string SessionTimesStr { get; set; }
    }
}