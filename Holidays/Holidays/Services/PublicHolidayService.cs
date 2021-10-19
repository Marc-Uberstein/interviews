using Holidays.Interfaces;
using Holidays.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Holidays.Services
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private const string Url = "https://date.nager.at/api/v2/publicholidays/";
        private readonly HttpClient _client;
        private IList<PublicHoliday> _data;

        public PublicHolidayService()
        {
            _client = new HttpClient {BaseAddress = new Uri(Url)};
            _data = null;
        }

        public async Task<IList<PublicHoliday>> GetByYearAsync(int year)
        {
            if (HasData()) return _data;

            var response = await _client.GetAsync($"{year}/ZA");
            return await ConvertResponse(response);
        }

        public async Task<IList<PublicHoliday>> GetByMonthAsync(int year, int month)
        {
            if (HasData()) return _data.Where(holiday => holiday.Date.Month == month).ToList();

            var response = await _client.GetAsync($"{year}/ZA");
            var holidays = await ConvertResponse(response);
            return holidays.Where(holiday => holiday.Date.Month == month).ToList();
        }

        private async Task<IList<PublicHoliday>> ConvertResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _data = JsonConvert.DeserializeObject<List<PublicHoliday>>(content);
            return _data;
        }

        private bool HasData()
        {
            return _data != null && _data.Count() > 0;
        }
    }
}