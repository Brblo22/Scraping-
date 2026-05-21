using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using skolmaten.Models;

namespace skolmaten.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            httpClient.DefaultRequestHeaders.Add("Client-Token",
                "web-eaa12e50-c84c-4b4a-9cfe-4e3fcbcd9165");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var today = DateTime.Today;
            int year = today.Year;
            int week = System.Globalization.ISOWeek.GetWeekOfYear(today);
            ViewData["Vecka"] = week;

            string url = $"https://skolmaten.se/api/4/menu/school/tullangsgymnasiet?year={year}&week={week}";
            string json = await httpClient.GetStringAsync(url);

            var results = new List<maträttModel>();

            using var doc = JsonDocument.Parse(json);
            var days = doc.RootElement.GetProperty("WeekState").GetProperty("Days");

            foreach (var day in days.EnumerateArray())
            {
                string dateStr = day.GetProperty("date").GetString() ?? "";
                var date = DateTime.Parse(dateStr);
                string dagNamn = date.ToString("dddd", new System.Globalization.CultureInfo("sv-SE"));
                dagNamn = char.ToUpper(dagNamn[0]) + dagNamn.Substring(1);

                var meals = day.GetProperty("Meals");
                var dagObj = new maträttModel { Dag = dagNamn };
                foreach (var meal in day.GetProperty("Meals").EnumerateArray())
                {
                    string maträtt = meal.GetProperty("name").GetString() ?? "";
                    dagObj.Maträtter.Add(maträtt);
                }
                results.Add(dagObj);
            }

            return View(results);
        }
    }
}