using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeWebApp.logic
{
    public class Helpers
    {
        public static string DisplayCodeString(string codeString)
        {
            string result = "";
            int count=0;
            foreach (char c in codeString)
            {
                if (c.ToString().ToUpper() == c.ToString() && count > 0)
                    result += " ";
                result += c.ToString();

                count++;
            }

            return result;
        }
    }
}