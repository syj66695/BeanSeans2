using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanSeans.Data;
using BeanSeans.Areas.Administration;
using Microsoft.AspNetCore.Identity;
using BeanSeans.Areas.Staff.Models.Reservation;

namespace BeanSeans.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ReservationController : AdministrationAreaController
    {

        public ReservationController(SignInManager<IdentityUser> sim, UserManager<IdentityUser> um, ApplicationDbContext _db) : base(sim, um, _db)
        {

        }


        //we have to have sittings to make reserv
        //when we add reservation, first we add siting
        //model: Sitting, List Tep
        public async Task<IActionResult> Sittings()
        {

            var sittings = await _db
                                 .Sittings
                                 .Where(s => s.End > DateTime.Now)//don't want to see past sittings
                                 .OrderBy(s => s.Start)

                                 .ToListAsync();
            return View(sittings);
        }
        //after sitting, user will be navigated to tables to choose area and table

        //create reservation after choose sitting.
        //manager has to choose member 
        //id==sitting Id
        [HttpGet]

        [HttpGet]
        public async Task<IActionResult> CreateMemberReservation(int id)
        {

            var sitting = await _db.Sittings
                                   .Include(s => s.SittingType)
                                   .FirstOrDefaultAsync(s => s.Id == id);
            if (sitting == null)
            {
                return NotFound();
            }
          

            var members = _db.Members.Select(me => new {
                Id = me.Id,
                Name = $"{me.LastName}, {me.FirstName}"
            }).ToList();

            var m = new CreateMemberReservation
            {


                StartTime = sitting.Start,

                SittingId = sitting.Id,
                Sitting = $"{sitting.SittingType.Name} {sitting.Start}",
                //StatusId = 1, //initial status is pending with id of 1
              //  StatusOptions = new SelectList(_db.ReservationStatuses.ToList(), "Id", "Name"),
                SourceOptions = new SelectList(_db.ReservationSources.ToList(), "Id", "Name"),
                MemberOptions = new SelectList(members, "Id", "Name")
            };



            return View(m);
        }

       
        //once manager chooses member to add reservation
        //it will show the reservation form
        //Id is member ID
        [HttpPost]
        public async Task<IActionResult> CreateMemberReservation(CreateMemberReservation m)
        {
            //get sitting from m
            var sitting = await _db.Sittings.FirstOrDefaultAsync(s => s.Id == m.SittingId);
            if (sitting==null)
            {
                return NotFound();


            }

            //get reservations from the specific sitting 
            var reservations = await _db.Reservations.Where(r => r.SittingId == sitting.Id).ToListAsync();
            //get sum of resvervations' guests  from the sitting

            var totalGuest = reservations.Sum(r => r.Guest);
            var possibleCapacity = sitting.Capacity - totalGuest;
           
         
            

            if (! ModelState.IsValid || possibleCapacity < m.Guest|| m.Guest<=0)
            {            //when the capacity is full or 

                if (possibleCapacity < m.Guest || m.Guest <= 0)
                {
                    ViewBag.Guest = "<script>alert('Invalid guest number')</script>";
                }
              //  var errors = ModelState.Values.SelectMany(v => v.Errors);
                m.SittingId = sitting.Id;
                return View(m);
            }
            else
            {
                var member = await _db.Members.FirstOrDefaultAsync(me => me.Id == m.MemberId);
                var r = new Reservation
                {

                    Guest = m.Guest,
                    Duration = m.Duration,
                    Note = m.Note,
                    SourceId = m.SourceId,
                    StartTime = m.StartTime,
                    StatusId = 1,
                    SittingId = sitting.Id

                };
                //connect reservation and person
                member.Reservations.Add(r);
                //save cahnge
                _db.SaveChanges();
                return RedirectToAction(nameof(Confirmation), new { id = r.Id });

            }
          

        }


        public async Task<IActionResult> Confirmation(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _db.Reservations
                .Include(r => r.Person)
                .Include(r => r.Sitting)
                   .ThenInclude(s=>s.SittingType)
                .Include(r => r.Source)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Staff/Reservation
        public async Task<IActionResult> Index()
        {

            var reservations = await _db.Reservations
                                        .Include(r => r.Person)
                                        .Include(r => r.Sitting)
                                        .ThenInclude(st => st.SittingType)
                                        .Include(r => r.Source)
                                        .Include(r => r.Status)
                                        .ToListAsync();

            return View(reservations);
        }

        // GET: Staff/Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Staff/Reservation/Create
    
        // GET: Staff/Reservation/Edit/5
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
            ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator", reservation.PersonId);
            ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id", reservation.SittingId);
            ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id", reservation.SourceId);
            ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id", reservation.StatusId);
            return View(reservation);
        }

        // POST: Staff/Reservation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PersonId,SittingId,StatusId,SourceId,Id,Guest,StartTime,Duration,Note")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

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
            ViewData["PersonId"] = new SelectList(_db.People, "Id", "Discriminator", reservation.PersonId);
            ViewData["SittingId"] = new SelectList(_db.Sittings, "Id", "Id", reservation.SittingId);
            ViewData["SourceId"] = new SelectList(_db.ReservationSources, "Id", "Id", reservation.SourceId);
            ViewData["StatusId"] = new SelectList(_db.ReservationStatuses, "Id", "Id", reservation.StatusId);
            return View(reservation);
        }

        // GET: Staff/Reservation/Delete/5
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

        // POST: Staff/Reservation/Delete/5
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
