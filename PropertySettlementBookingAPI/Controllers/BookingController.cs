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
        private readonly TimeSpan _endTime = new TimeSpan(16, 0, 0);
        private readonly Dictionary<string, string> bookings = new Dictionary<string, string>();
       

        [HttpPost]
        public IActionResult CreateBooking(Booking booking)
        {
            // Validate booking request
            if (!IsValidTime(booking.BookingTime) || string.IsNullOrWhiteSpace(booking.Name))
            {
                return BadRequest("Invalid booking data");
            }

            var time = TimeSpan.Parse(booking.BookingTime);

            if (time < _startTime || time >= _endTime)
            {
                return BadRequest("Booking time is outside of business hours (9am-5pm)");
            }            

            // Check if all settlements at the booking time are reserved
            if ( bookings.ContainsKey(booking.BookingTime) && (bookings[booking.BookingTime] != null))
            {
                return Conflict("Booking time is already reserved");

            }

            // Check if maximum booking count is reached above 4
            if (CountBooking() >= MaxSimultaneousSettlements)
            {
                return StatusCode(503, "Maximum simultaneous settlements reached");
            }

           
            // Make the booking
            var bookingId = Guid.NewGuid().ToString();
            bookings[booking.BookingTime] = booking.BookingTime;

            var cc = bookings.Count;
            return Ok(new { bookingId });           

        }

        private bool IsValidTime(string bookingTime)
        {
            if (string.IsNullOrWhiteSpace(bookingTime))
                return false;

            return TimeSpan.TryParse(bookingTime, out _);
        }


        private int CountBooking()
        {
            var count = 0;
            foreach (var booking in bookings.Values)
            {
                if (booking != null)
                    count++;
            }
            return count;
        }
    }


}
