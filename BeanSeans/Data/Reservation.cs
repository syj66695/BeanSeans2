using System;
using System.Collections.Generic;

namespace BeanSeans.Data
{
    public class Reservation
    {
        public int PersonId { get; set; }//1,1 relationship
        public Person Person { get; set; }//1 relationship

        public int SittingId { get; set; }//1 relationship

        public Sitting Sitting { get; set; }//1,1 relationship
        //1-relationship
        public int StatusId { get; set; }
        //1-relationship
        public ReservationStatus Status { get; set; }


        //1-relationship: FK
        public int SourceId { get; set; }
        public ReservationSource Source { get; set; }

        //m
        public List<TableReservation> TableReservations { get; set; }
        public Reservation()
        {
            TableReservations = new List<TableReservation>();
        }


        public int Id { get; set; }
    
        public int Guest { get; set; }

        public DateTime StartTime { get; set; }

        public int Duration { get; set; }


        public string Note { get; set; }

    }
}