using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace GiftMatch.Helpers
{
    public static class DropDownListHelper
    {
        public static MvcHtmlString DropDownListForMonths(this HtmlHelper helper, string name)
        {
            List<SelectListItem> months = new List<SelectListItem>();
            months.Add(new SelectListItem { Text = "Month", Value = "" });
            months.Add(new SelectListItem { Text = "January", Value = "1" });
            months.Add(new SelectListItem { Text = "February", Value = "2" });
            months.Add(new SelectListItem { Text = "March", Value = "3" });
            months.Add(new SelectListItem { Text = "April", Value = "4" });
            months.Add(new SelectListItem { Text = "May", Value = "5" });
            months.Add(new SelectListItem { Text = "June", Value = "6" });
            months.Add(new SelectListItem { Text = "July", Value = "7" });
            months.Add(new SelectListItem { Text = "August", Value = "8" });
            months.Add(new SelectListItem { Text = "September", Value = "9" });
            months.Add(new SelectListItem { Text = "October", Value = "10" });
            months.Add(new SelectListItem { Text = "November", Value = "11" });
            months.Add(new SelectListItem { Text = "December", Value = "12" });
            return helper.DropDownList(name, months);
        }
        public static MvcHtmlString DropDownListForDays(this HtmlHelper helper, string name)
        {
            List<SelectListItem> days = new List<SelectListItem>();

            days.Add(new SelectListItem { Text = "Day", Value = "" });

            for (int i = 1; i <= 31; i++)
                days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });            
            return helper.DropDownList(name, days);
        }
        public static MvcHtmlString DropDownListForYears(this HtmlHelper helper, string name)
        {
            List<SelectListItem> years = new List<SelectListItem>();

            years.Add(new SelectListItem { Text = "Year", Value = "" });

            for (int i = DateTime.Now.Year; i >= DateTime.Now.AddYears(-100).Year; i--)
                years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });

            return helper.DropDownList(name, years);
        }

        public static MvcHtmlString DropDownListForCountries(this HtmlHelper helper, string name)
        {
            List<SelectListItem> countries = new List<SelectListItem>();
            countries.Add(new SelectListItem { Value = "United States", Text = "United States" });
            countries.Add(new SelectListItem { Value = "Pakistan", Text = "Pakistan" });
            countries.Add(new SelectListItem { Value = "England", Text = "England" });
            countries.Add(new SelectListItem { Value = "Uganda", Text = "Uganda" });
            return helper.DropDownList(name, countries);
        }
        public static MvcHtmlString DropDownListForEthnicities(this HtmlHelper helper, string name)
        {
            List<SelectListItem> countries = new List<SelectListItem>();
            countries.Add(new SelectListItem { Value = "Caucasion", Text = "Caucasion" });
            countries.Add(new SelectListItem { Value = "African American", Text = "African American" });
            countries.Add(new SelectListItem { Value = "Asian", Text = "Asian" });
            countries.Add(new SelectListItem { Value = "Latin", Text = "Latin" });
            return helper.DropDownList(name, countries);
        }
    }
}