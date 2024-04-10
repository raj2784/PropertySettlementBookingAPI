using Microsoft.AspNetCore.Mvc;
using PropertySettlementBookingAPI.Model;
using System;
using System.Collections.Generic;

namespace SettlementBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private const int MaxSimultaneousSettlements = 4;
        private readonly TimeSpan _startTime = new TimeSpan(9, 0, 0);
        private readonly TimeSpan _endTime = new TimeSpan(17, 0, 0);
        private static Dictionary<DateTime, List<string>> bookings = new Dictionary<DateTime, List<string>>();



        [HttpPost]
        public IActionResult CreateBooking(Booking booking)
        {
            // Validate booking request
            if (!IsValidTime(booking.BookingTime) || string.IsNullOrWhiteSpace(booking.Name))
            {
                return BadRequest("Invalid booking input");
            }
            var time = TimeSpan.Parse(booking.BookingTime);

            if (time < _startTime || time >= _endTime)
            {
                return BadRequest("Booking time is outside of business hours (9am-5pm)");
            }


            var bookingDateTime = GetBookingDateTime(time);
            
            lock (bookings)
            {
                if (bookings.ContainsKey(bookingDateTime))
                {
                    var bookingsForTime = bookings[bookingDateTime];
                    if (bookingsForTime.Count >= MaxSimultaneousSettlements)
                        return Conflict("All settlements at this time are reserved");
                }
                else
                {
                    bookings[bookingDateTime] = new List<string>();
                }

                bookings[bookingDateTime].Add(booking.Name);
            }

            var bookingId = Guid.NewGuid();
            return Ok(new { BookingId = bookingId });

        }

        private bool IsValidTime(string bookingTime)
        {
            if (string.IsNullOrWhiteSpace(bookingTime))
                return false;

            return TimeSpan.TryParse(bookingTime, out _);
        }

        private DateTime GetBookingDateTime(TimeSpan bookingTime)
        {
            var today = DateTime.Today;
            return today.Add(bookingTime);
        }


    }

}
