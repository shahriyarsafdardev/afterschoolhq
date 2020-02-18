using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;

namespace AfterSchool.Models
{
    public class UI
    {
        public static string MakeLink(string url)
        {
            return url.Contains("http://") || url.Contains("https://") ? url : $"http://{url}";
        }

        public static string JavascriptSafeString(string value)
        {
            if (value.IsNotEmpty())
                return value.Replace("'", "\\'");

            return "";
        }

        public static string RenderMessagesToHtml(SystemMessages msgs)
        {
            string x = "";
            foreach(SystemMessage msg in msgs)
            {
                string alertType = "";
                switch(msg.MessageType)
                {
                    case MessageType.Success:
                        alertType = "success";
                        break;
                    case MessageType.Confirmation:
                        alertType = "info";
                        break;
                    case MessageType.Error:
                    case MessageType.UserError:
                        alertType = "danger";
                        break;
                    case MessageType.Notice:
                        alertType = "info";
                        break;
                    case MessageType.Warning:
                        alertType = "warning";
                        break;
                }
                x += string.Format("<div role=\"alert\" class=\"alert alert-{0}\">{1}</div>", alertType, msg.Text);
            }
            return x;
        }
    }
}
