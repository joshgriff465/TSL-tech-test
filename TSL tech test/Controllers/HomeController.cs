using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using TSL_tech_test.Models;
using TSL_tech_test.Services;
using System.Net.WebSockets;
using System.Text;



namespace TSL_tech_test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //initial page with data from most specific season
            HomeControllerModel homeControllerModel = await getDataAsync(null, "2022", null);
            return View(homeControllerModel);
        }
        [HttpPost]
        public async Task<ActionResult> Index(HomeControllerModel hcm)
        {
            //circuit is unused but was going to have it so it could alternate api calls
            var season = Request.Form["btnSubmit"];
            var circuit = Request.Form["btnCircuit"];

            HomeControllerModel homeControllerModel = await getDataAsync(null, season.ToString(), circuit);
            return View(homeControllerModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<HomeControllerModel> getDataAsync(string[] args , string season, string circuit)
        {
            //initialise
            HomeControllerModel hcm = new HomeControllerModel();
            DataService ds = new DataService();
            RaceInfoModel.Root raceInfo = new RaceInfoModel.Root();


            //goes to a data service to get the response from the server, fills in some arguments so users input can change the data they receive
            string serverResult = await ds.GetDataFromServerAsync(null, season, circuit);

            //deserialize the json data that was retrieved from the server
            raceInfo = JsonConvert.DeserializeObject<RaceInfoModel.Root>(serverResult);

            //more clean model to use speicific parts of the data
            hcm.DataSource = raceInfo.MRData.url;
            hcm.raceTable = raceInfo.MRData.RaceTable;
            hcm.Seasons = new List<int>{2022, 2021, 2020, 2019 };

            return hcm;

        }
    }





}