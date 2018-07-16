//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using AfterSchoolLibrary;
//using BasicLibrary;
//using AfterSchool.Models;
//using AfterSchool.Helpers;

//namespace AfterSchool.Controllers
//{
//    public class ServicesController : BaseController
//    {
//        // GET: Services

//        public ActionResult SetGrade(string id)
//        {
//            var msgs = new SystemMessages();
//            Response.Clear();
//            if (id.IsNumber())
//            {
//                if (SessionVariables.Cart.IsEmpty())
//                {
//                    SessionVariables.Cart = Cart.GetNewCart(msgs: msgs);
//                }
//                SessionVariables.Cart.GradeLevel = id.ToInteger();
//            }

//            HandleErrors(msgs, ResponseType.JSON);
//            Response.End();
//            return View();
//        }

//        public ActionResult SetGender(string id)
//        {
//            var msgs = new SystemMessages();
//            Response.Clear();
//            if (id.IsNumber())
//            {
//                if (SessionVariables.Cart.IsEmpty())
//                {
//                    SessionVariables.Cart = Cart.GetNewCart(msgs: msgs);
//                }
//                SessionVariables.Cart.Gender = (GenderType)id.ToInteger();
//            }

//            HandleErrors(msgs, ResponseType.JSON);
//            Response.End();
//            return View();
//        }

//        public ActionResult SetAge(string id)
//        {
//            var msgs = new SystemMessages();
//            Response.Clear();
//            if (id.IsNumber())
//            {
//                if (SessionVariables.Cart.IsEmpty())
//                {
//                    SessionVariables.Cart = Cart.GetNewCart(msgs: msgs);
//                }
//                SessionVariables.Cart.Age = id.ToInteger();
//            }

//            HandleErrors(msgs, ResponseType.JSON);
//            Response.End();
//            return View();
//        }

//        public ActionResult CartItemsGet()
//        {
//            var msgs = new SystemMessages();
//            Response.Clear();
//            if (msgs.HasNoErrors())
//            {
//                var list = new List<object>();
//                var items = new List<CartItem>();

//                if (SessionVariables.Cart.IsNotEmpty())
//                {
//                    items = SessionVariables.Cart.GetCartItems();
//                }

//                foreach(var x in items)
//                {
//                    var dows = new List<object>();
//                    var activity = Activity.Load(x.ActivityId.Value);

//                    foreach(var y in activity.GetDaysOfTheWeek())
//                    {
//                        dows.Add(new
//                        {
//                            Day = y.ToString(),
//                            SeatsRemaining = activity.GetSeatsRemaining(),
//                            WaitingListSeatsRemaining = activity.GetWaitingListSeatsRemaining(),                           
//                        });
//                    }

//                    list.Add(new
//                    {
//                        Name = activity.Name,
//                        Uid = activity.Uid,
//                        Days = dows,
//                        Cost = activity.CostPerRegistrant.ToCurrency().Replace("$", ""),
//                        IsRegistered = x.RegistrantActivityId.HasValue
//                    });
//                }
                
//                WriteOutput(new { status = "ok", cartItems = list }, ResponseType.JSON);
//            }
//            else
//            {
//                HandleErrors(msgs, ResponseType.JSON);
//            }
//            Response.End();
//            return View();
//        }

//        public ActionResult CartItemAdd(string uid)
//        {
//            var msgs = new SystemMessages();
//            Activity activity = null;

//            if (SessionVariables.Cart.IsEmpty())
//            {
//                SessionVariables.Cart = Cart.GetNewCart(msgs: msgs);
//            }

//            activity = Activity.LoadByUid(uid.ToGuid());
//            if (activity.IsNotEmpty())
//            {               
//                SessionVariables.Cart.AddCartItem(activity.ID.Value, DayOfWeekASL.NotSet, null, msgs);
//            }
//            else
//            {
//                msgs.Add("Invalid activity", MessageType.UserError);
//            }

//            Response.Clear();
//            if (msgs.HasNoErrors())
//            {
//                WriteOutput(new { status = "ok" }, ResponseType.JSON);
//            }
//            else
//            {
//                HandleErrors(msgs, ResponseType.JSON);
//            }
//            Response.End();

//            return View();
//        }

//        public ActionResult CartItemRemove(string uid)
//        {
//            var msgs = new SystemMessages();

//            if (SessionVariables.Cart.IsNotEmpty())
//            {
//                var items = SessionVariables.Cart.GetCartItems();
//                var activity = Activity.LoadByUid(uid.ToGuid());

//                if (activity.IsNotEmpty() && items.Count > 0)
//                {
//                    var item = items.Where(x => x.ActivityId == activity.ID.Value).FirstOrDefault();
//                    if (item.IsNotEmpty())
//                    {
//                        msgs.AddRange(item.Delete());
//                        SessionVariables.Cart.ResetCartItems();
//                    }
//                }
//            }

//            Response.Clear();
//            if (msgs.HasNoErrors())
//            {
//                WriteOutput(new { status = "ok" }, ResponseType.JSON);
//            }
//            else
//            {
//                HandleErrors(msgs, ResponseType.JSON);
//            }
//            Response.End();

//            return View();
//        }

//        public ActionResult SubmitContactForm(string email, string subject, string message, string g_recaptcha_response)
//        {
//            var msgs = new SystemMessages();

//            var status = "";

//            if (!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
//            {
//                msgs.Add("Please enter an email", MessageType.UserError);
//            }
//            if (subject.IsBlank())
//            {
//                msgs.Add("Please enter a subject", MessageType.UserError);
//            }
//            if (message.IsBlank())
//            {
//                msgs.Add("Please enter a message", MessageType.UserError);
//            }

//            msgs.AddRange(RecaptchaHelper.Validate(g_recaptcha_response));

//            if (msgs.HasNoErrors())
//            {
//                /*
//                var msg = new System.Net.Mail.MailMessage();
//                msg.To.Add(AfterSchoolLibrary.Email.AppAdminEmailAddress);
//                msg.From = new System.Net.Mail.MailAddress(AfterSchoolLibrary.Email.AppEmailAddress);
//                msg.Subject = string.Format("WEBSITE FORM: {0}", subject);
//                msg.Body = string.Format("<p>from: <a href=\"mailto:{0}\">{0}</a></p><p>{1}</p>", email, message);
//                msg.IsBodyHtml = true;

//                AfterSchoolLibrary.Email.SendMail(msg);
//                */

//                AfterSchoolLibrary.SendGridHelper.SendMail(
//                    fromEmail: AfterSchoolLibrary.Email.AppEmailAddress,
//                    toEmail: AfterSchoolLibrary.Email.AppAdminEmailAddress,
//                    subject: string.Format("WEBSITE FORM: {0}", subject),
//                    body: string.Format("<p>from: <a href=\"mailto:{0}\">{0}</a></p><p>{1}</p>", email, message)
//                );

//                status = "ok";
//            }
//            else
//            {
//                status = "error";
//            }

//            return new JsonResult { Data = new { status = status, messages = msgs } };
//        }
//    }
//}