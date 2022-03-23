using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using static AfterSchoolLibrary.Helpers.GoogleAutocomplete;
using Location = AfterSchoolLibrary.Models.Location;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220323_2_LatitudeLongitudeRefetch
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var key = ConfigurationManager.AppSettings["Google.AutoComplete.ApiKey"];
                    var locations = Models.Location.Load();
                    foreach (var item in locations.Where(m => m.GooglePlaceId.IsNotEmpty()))
                    {                        
                        HttpClient client = new HttpClient();
                        var response = client.GetStringAsync($"https://maps.googleapis.com/maps/api/place/details/json?placeid={item.GooglePlaceId}&key={key}");

                        Root root = JsonConvert.DeserializeObject<Root>(response.Result);

                        var latitude = root.status == "OK" ? root.result?.geometry?.location?.lat : 0;
                        var longitude = root.status == "OK" ? root.result?.geometry?.location?.lng : 0;
                        var distanceFromOrigin = DistanceAlgorithm.DistanceBetweenPlaces(longitude.Value, latitude.Value, 0, 0);

                        item.Latitude = Convert.ToDecimal(latitude);
                        item.Longitude = Convert.ToDecimal(longitude);
                        item.DistanceFromOrigin = Convert.ToDecimal(distanceFromOrigin);

                        item.Save(adc);
                    }
                },
                uniqueIdenfier: "Migration_20220323_2_LatitudeLongitudeRefetch",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
