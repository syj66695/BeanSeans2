using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BeanSeans.Data
{
    public class Person
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]

        public string LastName { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        

        public string Email { get; set; }
        [Required, DataType(DataType.PhoneNumber)]

        public string Phone { get; set; }
       

        //nullable-AspUser
        public string UserId { get; set; }//Guest


        public virtual bool IsMember

        {
            get { return false; }
        }

        public virtual bool IsStaff

        {
            get { return false; }
        }

        //1 relationship
        public Restuarant Restaurant { get; set; }
        //FK
        public int RestuarantId { get; set; }


        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

   
    }

}
