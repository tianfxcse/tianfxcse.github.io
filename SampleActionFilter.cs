using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VTS.Mobile
{
    public class SampleActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string sLang = "1";
            var lang = context.HttpContext.Session["language"];
            string newLang = context.HttpContext.Request.QueryString["lang"];
            if(!string.IsNullOrEmpty(newLang))
            {
                sLang = newLang == "0" ? "0" : "1";
            }
            else if (lang == null)
            {
                sLang = "1";
            }
            else
            {
                sLang = lang.ToString() == "0" ? "0" : "1";
            }
            context.HttpContext.Session["language"] = sLang;

            string lang_txt = sLang == "0" ? "中文" : " EN";
            context.Controller.ViewData["lang_txt"] = lang_txt;
            context.Controller.ViewData["AppTitle"] = Translanter.GetLanguageMsg("AppTitle", sLang);
            context.Controller.ViewData["DataPolicy"] = Translanter.GetLanguageMsg("DataPolicy", sLang);
            context.Controller.ViewData["BannerText"] = Translanter.GetLanguageMsg("BannerText", sLang);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
        }
    }
}