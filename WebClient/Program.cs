using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HomeAppsLib;

namespace ConsoleService
{
    class Program
    {
        static int _exceptionCount = 0;
        const int EXCEPTION_THRESHOLD = 30;
        const int RUN_INVERVAL_MINUTES = 15;

        static void Main(string[] args)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            while (true)
            {
                try
                {
                    // 1. update data
                    bool changesMade = LibCommon.UpdateNFLMatchups(false);

                    // 2. try force refresh cache
                    if (changesMade)
                    {
                        try
                        {
                            string url = LibCommon.WebsiteUrlRoot() + "LoadCache.aspx?key=true";
                            client.DownloadString(url);
                        }
                        catch { }
                    }

                    // 3. report
                    if (LibCommon.IsAppInTestMode())
                        LibCommon.SendEmail("eric.wright@jvic.com", "Console Service Successful", "At " + DateTime.Now, "Console Service");

                    // 4. wait
                    System.Threading.Thread.Sleep(1000 * 60 * RUN_INVERVAL_MINUTES); 
                }
                catch (Exception ex)
                {
                    try
                    {
                        _exceptionCount++;
                        string body = ex.ToString();

                        if (_exceptionCount == EXCEPTION_THRESHOLD)
                            body = "PROGRAM TERMINATED AFTER " + EXCEPTION_THRESHOLD + " ATTEMPTS<br /><br />" + body;

                        LibCommon.SendEmail("eric.wright@jvic.com", "Console Service Exception", body, "Console Service");

                        if (_exceptionCount == EXCEPTION_THRESHOLD)
                            return;
                    }
                    catch (Exception ex2)
                    {
                        string message = "Ex1={" + ex.Message + "}\n\nEx2={" + ex2.Message + "}";
                        Console.WriteLine(message);
                        Console.ReadLine();
                        return; 
                    }
                }
            }
        }
    }
}
