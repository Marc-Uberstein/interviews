using Holidays.Data;
using Holidays.Interfaces;
using Holidays.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Holidays.Controllers
{
    [Route("api/DaysOff")]
    public class DaysOffController : Controller
    {
        private readonly IPublicHolidayService _publicHolidayService;
        private readonly HolidayContext _holidayContext;

        public DaysOffController(IPublicHolidayService publicHolidayService, HolidayContext holidayContext)
        {
            _publicHolidayService = publicHolidayService;
            _holidayContext = holidayContext;
        }

        [HttpGet("{year}")]
        public async Task<IActionResult> GetHolidays(int year)
        {
            var holidays = new List<HolidayDto>();
            var publicHolidays = await _publicHolidayService.GetByYearAsync(year);
            var workHolidays = (await _holidayContext.Holidays.ToListAsync()).Where(x => x.Date.Year == year);

            ShiftHolidayToNextAvailableWorkDay(workHolidays, publicHolidays);

            holidays.AddRange(publicHolidays.Select(x => new HolidayDto(x.Date, x.Name)));
            holidays.AddRange(workHolidays.Select(x => new HolidayDto(x.Date, x.Name)));

            return Ok(holidays.OrderBy(x => x.Date));

        }

        public static void ShiftHolidayToNextAvailableWorkDay(IEnumerable<Data.Entities.Holiday> list, IList<PublicHoliday> publicHolidays)
        {
            foreach (var holiday in list)
            {
                while (holiday.Date.DayOfWeek == DayOfWeek.Saturday
                    || holiday.Date.DayOfWeek == DayOfWeek.Sunday
                    || publicHolidays.Any(x => x.Date == holiday.Date))
                {
                    holiday.Date = holiday.Date.AddDays(1);
                }
            }
        }
    }
}
