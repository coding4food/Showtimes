using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showtimes.Domain
{
    public class Showtimes
    {
        public int MovieTheaterId { get; private set; }
        public int MovieId { get; private set; }
        public DateTime SessionTime { get; private set; }

        public virtual MovieTheater Theater { get; private set; }
        public virtual Movie Movie { get; private set; }

        internal Showtimes(int movieTheaterId, int movieId, DateTime sessionTime)
        {
            this.MovieTheaterId = movieTheaterId;
            this.MovieId = movieId;
            this.SessionTime = sessionTime;
        }
    }
}
