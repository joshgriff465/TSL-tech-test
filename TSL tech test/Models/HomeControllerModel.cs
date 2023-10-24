namespace TSL_tech_test.Models
{
    public class HomeControllerModel
    {
        public string DataSource { get; set; }

        public RaceInfoModel.RaceTable raceTable { get; set; }

        public List<int> Seasons { get; set; }
        
    }
}
