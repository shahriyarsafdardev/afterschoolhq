using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Controllers
{
    public class DbController : BaseController
    {
        private readonly DatabaseCredentials databaseCredentials;

        public DbController(DatabaseCredentials databaseCredentials)
        {
            this.databaseCredentials = databaseCredentials;
        }

        [HttpGet]
        public ActionResult Update(string id)
        {
            if (Request.UserHostAddress != "::1" && id !=
                "eibfeiwalncdsja32rq9yfwbfg32yewody7832re7ywofaudisfh7238qoweiafb7328qwoadg78027rweubfad")
                return Json(new {status = "ok"}, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            msgs.AddRange(DatabaseInit.InitDatabase(databaseCredentials, "darye@afterschoolhq.com",
                "help students learn", "317-316-0596", false, "AfterSchool HQ"));

            return Json(new {status = "ok", messages = msgs.ToList().Select(x => new {x.MessageType, x.Text})},
                JsonRequestBehavior.AllowGet);
        }
    }
}