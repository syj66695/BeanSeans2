using System;
using System.Collections.Generic;

namespace BeanSeans.Data
{
    public class Sitting
    {
        //1-relationship
        public SittingType SittingType { get; set; }
        //FK
        public int SittingTypeId { get; set; }

        //1-relationship
        public Restuarant Restuarant { get; set; }
        //FK
        public int RestuarantId { get; set; }

        //m-relationship
        public List<Reservation> Reservations { get; set; }

        public Sitting()
        {//instanciate
            Reservations = new List<Reservation>();
        }

        //Prop
        public int Id { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int Capacity { get; set; }




    }
}