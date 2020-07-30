using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index(string city, string state, string neighborhoods, string covid)
        {
            var msgs = new SystemMessages();

            if (city.IsEmpty())
                city = "Indianapolis";

            if (state.IsEmpty())
                state = "Indiana";

            List<string> neighborhoodSearch = null;

            if (neighborhoods.IsNotEmpty())
                neighborhoodSearch = neighborhoods.Split(',').ToList();

            var providers = AccountProfile.Search(
                                accountTypes: AccountService.CustomerAccountTypes,
                                isOpenDuringCovid: covid.IsEmpty() ? new bool?() : covid.ToBoolean(),
                                neighborhoods: neighborhoodSearch,
                                cities: city.AsList(),
                                states: state.AsList(),
                                msgs: msgs);

            var neighborhoodList = Location.LoadAllNeighborhoods(
                                        city: city,
                                        state: state,
                                        msgs: msgs)
                                    .Where(x => x.IsNotEmpty())
                                    .ToList();

            ViewBag.Messages = msgs;
            ViewBag.Neighborhoods = neighborhoodList;
            ViewBag.City = city;
            ViewBag.State = state;

            return View(providers.Shuffle().ToList());
        }
    }
}