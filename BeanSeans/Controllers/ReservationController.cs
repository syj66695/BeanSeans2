using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanSeans.Data;
using Microsoft.AspNetCore.Identity;
using BeanSeans.Models.Reservation;

namespace BeanSeans.Controllers
{
    public class ReservationController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly ApplicationDbContext _db;

        public ReservationController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        //we have to have sittings to make reserv
        //when we add reservation, first we add siting
        //model: Sitting, List Tep
        public  async Task<IActionResult> Sittings()
        {
            var sittings = await _db.Sittings
                                   .Include(s => s.SittingType)
                                       .OrderBy(s =>s.Start)
                                        .ToListAsync();
            return View(sittings);
        }
        //after sitting, user will be navigated to tables to choose area and table
      
        // GET: Reservation
        public async Task<IActionResult> Index()
        {
            var reservation = await _db.Reservations
                .Include(r => r.Person)
                  .Include(r => r.Sitting)
                     .ThenInclude(s => s.SittingType)
                      .Include(r => r.Source)
                          .Include(r => r.Status).ToListAsync();
                
            return View( reservation);
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations
                .Include(r => r.Person)
                .Include(r => r.Sitting)
                .ThenInclude(s => s.SittingType)
                .Include(r => r.Source)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        [HttpGet]//id= selected sitting id
        public async Task<IActionResult> Create(int id)
        { //find sitting by id
            var sitting = await _db.Sittings.FirstOrDefaultAsync(s => s.Id == id);

            if (sitting == null)
            {
                //validation
                return NotFound();
            }
            var m = new CreateReservation
            {
                Sitting = sitting,
                Time = sitting.Start,//put the default time as a start time
               
            };
            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {//get user if it is member
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                //get person from Created reservation 
                m.Person = await _db.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
            }
            return View(m);

            //ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator");
            //ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id");
            //ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id");
            //ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id");
            //return View();
        }

        // POST: Reservation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(CreateReservation m)
        {
            var sitting = await _db.Sittings.FirstOrDefaultAsync(s => s.Id == m.Sitting.Id);

            if (sitting == null)
            {
                return NotFound();
            }
            if (m.Time < sitting.Start || m.Time > sitting.End)
            {
                ModelState.AddModelError("Time", "Invalid Time, must fall within sitting start/end times");
            }
            if (ModelState.IsValid) {
                bool isMember = false;
                Person p;
                if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
                {
                    //get user by name
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);

                    p = await _db.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
                    isMember = true;

                }
                else//user not login and not member
                {
                    p = new Person
                    {
                        FirstName = m.Person.FirstName,
                        LastName = m.Person.LastName,
                        Email = m.Person.Email,
                        Phone = m.Person.Phone,
                        RestuarantId = 1
                        

                    };

                    _db.People.Add(p);
                }

                //make new reservation
                var r = new Reservation
                {
                    Guest = m.Guest,
                    StartTime = m.Time,
                    SittingId = sitting.Id,
                    Duration = m.Duration,
                    Note = m.Note,
                    StatusId = 1,//pending
                    SourceId = 1,///Online

                };

                //connect reservation and person
                p.Reservations.Add(r);
                //save cahnge
                _db.SaveChanges();

                if (isMember)
                {
                    return RedirectToAction("Index", "Reservation", new { Area = "Member" });
                }

                return RedirectToAction(nameof(Details), new { id = r.Id });
            }



            //ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator", reservation.PersonId);
            //ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id", reservation.SittingId);
            //ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id", reservation.SourceId);
            //ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id", reservation.StatusId);
            //return View(reservation);

            //if we got so far, then the model is not valid sp send back create form
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            m.Sitting = sitting;
            return View(m);
        }
        [HttpGet]
        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations.FindAsync(id);
                

            if (reservation == null)
            {
                return NotFound();
            }

            //ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator", reservation.PersonId);
            //ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id", reservation.SittingId);
            //ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id", reservation.SourceId);
            //ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id", reservation.StatusId);

            return View(reservation);
        }

        // POST: Reservation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Reservation reservation)
        {
            

            if (ModelState.IsValid)
            {
                try
                {
                   
                    _db.Update(reservation);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(reservation);
        }

        // GET: Reservation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations
                .Include(r => r.Person)
                .Include(r => r.Sitting)
                .Include(r => r.Source)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _db.Reservations.Any(e => e.Id == id);
        }
    }
}
