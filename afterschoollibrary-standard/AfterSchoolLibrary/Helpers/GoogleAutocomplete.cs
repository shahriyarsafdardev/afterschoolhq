using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using BasicLibrary;
using Newtonsoft.Json;

namespace AfterSchoolLibrary.Helpers
{
    public class GoogleAutocomplete
    {
        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Northeast
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Viewport
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
            public Viewport viewport { get; set; }
        }

        public class Photo
        {
            public int height { get; set; }
            public List<string> html_attributions { get; set; }
            public string photo_reference { get; set; }
            public int width { get; set; }
        }

        public class PlusCode
        {
            public string compound_code { get; set; }
            public string global_code { get; set; }
        }

        public class Review
        {
            public string author_name { get; set; }
            public string author_url { get; set; }
            public string language { get; set; }
            public string profile_photo_url { get; set; }
            public int rating { get; set; }
            public string relative_time_description { get; set; }
            public string text { get; set; }
            public int time { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public string adr_address { get; set; }
            public string business_status { get; set; }
            public string formatted_address { get; set; }
            public string formatted_phone_number { get; set; }
            public Geometry geometry { get; set; }
            public string icon { get; set; }
            public string international_phone_number { get; set; }
            public string name { get; set; }
            public List<Photo> photos { get; set; }
            public string place_id { get; set; }
            public PlusCode plus_code { get; set; }
            public double rating { get; set; }
            public string reference { get; set; }
            public List<Review> reviews { get; set; }
            public List<string> types { get; set; }
            public string url { get; set; }
            public int user_ratings_total { get; set; }
            public int utc_offset { get; set; }
            public string vicinity { get; set; }
            public string website { get; set; }
        }

        public class Root
        {
            public List<object> html_attributions { get; set; }
            public Result result { get; set; }
            public string status { get; set; }
        }
    }

    public class GoogleGeoCode
    {
        public static (decimal latitude, decimal longitude) GetLatitudeAndLongitude(string zipcode, string address = "")
        {
            var key = ConfigurationManager.AppSettings["Google.AutoComplete.ApiKey"];
            var client = new HttpClient();
            var searchString = address.IsNotEmpty()
                ? $"address={HttpUtility.UrlEncode(address)}&components=postal_code:{zipcode.UrlEncode()}"
                : $"address=zip%20code%20{zipcode.UrlEncode()}";
            var response = client.GetStringAsync(
                $"https://maps.googleapis.com/maps/api/geocode/json?key={key}&{searchString}");

            //..Google geo code
            var root = JsonConvert.DeserializeObject<Root>(response.Result);

            var latitude = root.status == "OK" ? root.results?.FirstOrDefault()?.geometry?.location?.lat : 0;
            var longitude = root.status == "OK" ? root.results?.FirstOrDefault()?.geometry?.location?.lng : 0;

            return (Convert.ToDecimal(latitude.Value), Convert.ToDecimal(longitude.Value));
        }

        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Northeast
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Bounds
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Viewport
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Geometry
        {
            public Bounds bounds { get; set; }
            public Location location { get; set; }
            public string location_type { get; set; }
            public Viewport viewport { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
            public string place_id { get; set; }
            public List<string> types { get; set; }
        }

        public class Root
        {
            public List<Result> results { get; set; }
            public string status { get; set; }
        }
    }
}