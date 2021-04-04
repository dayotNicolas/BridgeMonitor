using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using BridgeMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;
using System.Text;

namespace BridgeMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var nextBoat = GetNextBoatCloseUp();

            return View(nextBoat);
        }

        public IActionResult AllCloses()
        {
            var arrayBoats = GetBoatsInfosFromApi();
            var boats = arrayBoats.OrderBy(closing=>Convert.ToDateTime(closing.ClosingDate)).ToList();
            return View(boats);
        }

        public IActionResult Calendar()
        {
            var arrayBoats = GetBoatsInfosFromApi();
            return View(arrayBoats);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult DownloadCalendar()
        {
            List<Boat> boats = GetBoatsInfosFromApi();
            for (int i = 0; i < boats.Count; i++)
            {
                if (boats[i].ClosingDate.CompareTo(DateTime.Now) < 0)
                {
                    boats.RemoveAt(i);
                }
            }
       
            boats.Sort((s1, s2) => DateTimeOffset.Compare(s1.ClosingDate, s2.ClosingDate));

            var calendar = new Calendar();
            foreach (Boat boat in boats)
            {
                var icalEvent = new Event
                {
                    Name = boat.BoatName,
                    Description = string.Format("Type {0}", boat.ClosingType),
                    Start = new CalDateTime(boat.ClosingDate),
                    End = new CalDateTime(boat.ReopeningDate)
                };

                calendar.Events.Add(icalEvent);
            }

            var iCalSerializer = new CalendarSerializer();
            string result = iCalSerializer.SerializeToString(calendar);

            return File(Encoding.ASCII.GetBytes(result), "calendar/text", "calendar.ics");
        }

        private static List<Boat> GetBoatsInfosFromApi()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("https://api.alexandredubois.com/pont-chaban/api.php");

                var stringResult = response.Result.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<List<Boat>>(stringResult.Result);
                return result;
            }
        }

        private static Boat GetNextBoatCloseUp()
        {
            
            DateTime Today = DateTime.Today;
            DateTime next = new DateTime(3000, 1, 1);
            var boats = GetBoatsInfosFromApi();
            Boat nextBoat = null;

            foreach (Boat boat in boats)
            {
                if (boat.ClosingDate >= Today && (boat.ClosingDate < next))
                {
                    next = boat.ClosingDate;
                } 
            }

            foreach (Boat boat in boats)
            {
                if (boat.ClosingDate == next)
                {
                    nextBoat = boat;
                }
            }

            return nextBoat;
        }
    }
}
