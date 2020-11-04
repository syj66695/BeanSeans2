using BeanSeans.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeanSeans.Models.Reservation
{
    public class CreateReservation
    {
        public Data.Person Person { get; set; }

        public Sitting Sitting { get; set; }

        public ReservationSource Source { get; set; }


        public int Guest { get; set; }

        public DateTime Time { get; set; }

        public int Duration { get; set; }


        public string Note { get; set; }




    }
}
