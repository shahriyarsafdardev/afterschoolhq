using System.Collections;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchool.Models.ActionFilters;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Controllers
{
    [EnvironmentRestrictionAttribute]
    public class SitemapController : BaseController
    {
        // GET: Sitemap
        [HttpGet]
        public ActionResult GenerateIndex()
        {
            var msgs = new SystemMessages();
            var lastModified = AccountProfile.GetLastUpdatedProfile(msgs);

            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.Append("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            xml.Append("<sitemap>");
            xml.Append($"<loc>{AfterSchoolLibrary.Helpers.Utilities.BaseUrl()}/sitemap-home.xml</loc>");
            xml.Append($"<lastmod>{lastModified.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
            xml.Append("</sitemap>");
            xml.Append("<sitemap>");
            xml.Append($"<loc>{AfterSchoolLibrary.Helpers.Utilities.BaseUrl()}/sitemap-profiles.xml</loc>");
            xml.Append($"<lastmod>{lastModified.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
            xml.Append("</sitemap>");
            xml.Append("</sitemapindex>");

            Response.Clear();
            Response.ClearHeaders();
            Response.AddHeader("content-type", "text/xml");
            Response.Write(xml.ToString());
            Response.End();          

            return View();
        }

        [HttpGet]
        public ActionResult GenerateProfileSitemap()
        {
            var profiles = AccountProfile.Load(criterias: new Hashtable() { { "AccountIsDeleted", false }, { "AccountIsTest", false }, { "IsAHQApproved", true } });
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            profiles.ForEach(profile =>
            {
                if (!profile.AccountIsDeleted && !profile.AccountIsTest && profile.IsAHQApproved)
                {
                    var account = profile.GetAccount();
                    var participationObjects = account.GetAllPublishedParticipationObjects().Where(x => !x.IsInviteOnly).ToList();

                    xml.Append("<url>");
                    xml.Append($"<loc>{account.HomePageUrl}</loc>");
                    xml.Append($"<lastmod>{profile.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
                    xml.Append("<changefreq>monthly</changefreq>");
                    xml.Append("<priority>.8</priority>");
                    xml.Append("</url>");

                    xml.Append("<url>");
                    xml.Append($"<loc>{account.BrowseUrl}</loc>");
                    xml.Append($"<lastmod>{profile.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
                    xml.Append("<changefreq>weekly</changefreq>");
                    xml.Append("<priority>.7</priority>");
                    xml.Append("</url>");

                    participationObjects.ForEach(participationObject =>
                    {
                        xml.Append("<url>");
                        xml.Append($"<loc>{participationObject.GetBrowseUrl()}</loc>");
                        xml.Append($"<lastmod>{participationObject.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
                        xml.Append("<changefreq>monthly</changefreq>");
                        xml.Append("<priority>.6</priority>");
                        xml.Append("</url>");
                    });
                }
            });

            xml.Append("</urlset>");

            Response.Clear();
            Response.ClearHeaders();
            Response.AddHeader("content-type", "text/xml");
            Response.Write(xml.ToString());
            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult GetHomeSitemap()
        {
            return File(Server.MapPath("~/sitemap-home.xml"), "text/xml");
        }
    }
}