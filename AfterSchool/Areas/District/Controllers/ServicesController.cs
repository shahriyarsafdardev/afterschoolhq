using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchool.Helpers;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.District.Controllers
{
    public class ServicesController : BaseLoggedInController
    {

        public ServicesController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

    }
}
