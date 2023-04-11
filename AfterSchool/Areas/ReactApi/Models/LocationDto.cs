namespace AfterSchool.Areas.ReactApi.Models
{
    public class LocationDto
    {
        public string locationName { get; set; }
        public string streetNumber { get; set; }
        public string streetName { get; set; }
        public string address2 { get; set; }
        public string neighborhood { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public string googlePlaceId { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
    }
}