using Holidays.Data;
using Holidays.Data.Entities;
using Holidays.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Holidays.IntegrationTests.Setup
{
    public class IntegrationTest
    {
        protected readonly HttpClient TestClient;
        protected readonly IServiceProvider ServiceProvider;

        public IntegrationTest()
        {
            var appFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(HolidayContext));
                    services.AddDbContext<HolidayContext>(options => options.UseInMemoryDatabase(databaseName: "CompanyHoliday"));
                });
            });
            TestClient = appFactory.CreateClient();

            ServiceProvider = appFactory.Services;

            SeedDb(2021);
            SeedDb(2020);
            SeedDb(2019);
        }

        private void SeedDb(int year)
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HolidayContext>();
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.InceptionDay, Date = new DateTime(year, 04, 05) });
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.JeffsBirthday, Date = new DateTime(year, 03, 26) });
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.MsIgniteDay, Date = new DateTime(year, 10, 13) });
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.PolkadotDay, Date = new DateTime(year, 08, 08) });
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.CreativeDay, Date = new DateTime(year, 01, 18) });
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.SpaceDay, Date = new DateTime(year, 07, 19) });
            context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), Name = HolidayNames.BmwDay, Date = new DateTime(year, 04, 25) });
            context.SaveChanges();
        }

        protected async Task<IList<HolidayDto>> ConvertResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<HolidayDto>>(content);
        }
    }
}