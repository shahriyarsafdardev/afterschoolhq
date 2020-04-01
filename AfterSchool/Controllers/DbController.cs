using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary;
using System.Text.RegularExpressions;
using System.Collections;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Text;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class DbController : BaseController
    {
        [HttpGet]
        public ActionResult Update(string id)
        {
            if (Request.UserHostAddress == "::1" || id == "eibfeiwalncdsja32rq9yfwbfg32yewody7832re7ywofaudisfh7238qoweiafb7328qwoadg78027rweubfad")
            {
                var msgs = new SystemMessages();

                msgs.AddRange(AfterSchoolLibrary.Helpers.Utilities.InitDatabase(
                    "darye@afterschoolhq.com", 
                    "help students learn", 
                    "317-316-0596", 
                    false, 
                    "AfterSchool HQ"));

                return Json(new { status = "ok", messages = msgs.ToList().Select(x => new { x.MessageType, x.Text } ) }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateSitemap(string id)
        {
            if (Request.UserHostAddress == "::1" || id == "eibfeiwalncdsja32rq9yfwbfg32yewody7832re7ywofaudisfh7238qoweiafb7328qwoadg78027rweubfad")
            {
                var msgs = new SystemMessages();

                try
                {
                    var indexXml = new StringBuilder();
                    indexXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    indexXml.Append("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                    indexXml.Append("<sitemap>");
                    indexXml.Append("<loc>https://afterschoolhq.com/sitemap-home.xml</loc>");
                    indexXml.Append($"<lastmod>{DateTime.UtcNow.ToRfc3339String()}</lastmod>");
                    indexXml.Append("</sitemap>");
                    indexXml.Append("<sitemap>");
                    indexXml.Append("<loc>https://afterschoolhq.com/sitemap-profiles.xml</loc>");
                    indexXml.Append($"<lastmod>{DateTime.UtcNow.ToRfc3339String()}</lastmod>");
                    indexXml.Append("</sitemap>");
                    indexXml.Append("</sitemapindex>");

                    System.IO.File.WriteAllText(Server.MapPath("~/sitemap_index.xml"), indexXml.ToString());

                    var profiles = AccountProfile.Load(criterias: new Hashtable() { { "AccountIsDeleted", false }, { "AccountIsTest", false } });
                    var xml = new StringBuilder();
                    xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    xml.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

                    profiles.ForEach(profile =>
                    {
                        if (!profile.AccountIsDeleted && !profile.AccountIsTest)
                        {
                            var account = profile.GetAccount();
                            var participationObjects = account.GetPublishedParticipationObjects().Where(x => !x.IsInviteOnly).ToList();

                            xml.Append("<url>");
                            xml.Append($"<loc>{account.HomePageUrl}</loc>");
                            xml.Append($"<lastmod>{profile.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
                            xml.Append("<changefreq>monthly</changefreq>");
                            xml.Append("<priority>.9</priority>");
                            xml.Append("</url>");

                            xml.Append("<url>");
                            xml.Append($"<loc>{account.BrowseUrl}</loc>");
                            xml.Append($"<lastmod>{profile.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
                            xml.Append("<changefreq>weekly</changefreq>");
                            xml.Append("<priority>.8</priority>");
                            xml.Append("</url>");

                            participationObjects.ForEach(participationObject => 
                            { 
                                xml.Append("<url>");
                                xml.Append($"<loc>{participationObject.GetBrowseUrl()}</loc>");
                                xml.Append($"<lastmod>{participationObject.LastModifiedDate.Value.ToRfc3339String()}</lastmod>");
                                xml.Append("<changefreq>monthly</changefreq>");
                                xml.Append("<priority>.7</priority>");
                                xml.Append("</url>");
                            });
                        }
                    });

                    xml.Append("</urlset>");

                    System.IO.File.WriteAllText(Server.MapPath("~/sitemap-profiles.xml"), xml.ToString());

                } 
                catch(Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error, ex);
                    AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(msgs);
                }
            }

            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }
    }
}
