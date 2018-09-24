using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchool.Helpers;

namespace AfterSchool.Areas.District.Controllers
{
    public class ServicesController : BaseLoggedInController
    {

        public ServicesController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }
        // GET: Services
        [HttpGet]
        public ActionResult SearchLocations(string search)
        {
            var msgs = new SystemMessages();
            var locations = Location.Search(search.Trim(), 0, 10, msgs);

            Response.Clear();
            WriteOutput(new
            {
                status = "ok",
                locations = locations.Select(location => new
                {
                    location.ID,
                    location.Name,
                    location.Address1,
                    location.Address2,
                    location.City,
                    location.State,
                    location.PostalCode,
                    FullAddress = location.FormatAddress(useLineBreaks: false),
                    location.HostAccountId,
                    LocationType = location.LocationTypeId.ToString()
                })
            },
            ResponseType.JSON);
            Response.End();

            return View();
        }

    }
}
