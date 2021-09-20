using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using HomeAppsLib;


namespace BatchService
{
    public partial class Service : ServiceBase
    {
        static int _exceptionCount = 0;
        const int EXCEPTION_THRESHOLD = 30;
        //const int RUN_INVERVAL_MINUTES = 15;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //LibCommon.SendEmail("eric@hackerdevs.com", "Batch Service Started", "Before DoWork", "Console Service");
            bgw.RunWorkerAsync();
            //LibCommon.SendEmail("eric@hackerdevs.com", "Batch Service Started", "After DoWork", "Console Service");
        }

        protected override void OnStop()
        {
            bgw.CancelAsync();
            LibCommon.SendEmail("eric@hackerdevs.com", "Batch Service Stopped", "Batch Service Stopped", "Console Service");
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            DoWork();
        }

        private void DoWork()
        {
            System.Net.WebClient client = new System.Net.WebClient();
            while (true)
            {
                if (bgw.CancellationPending)
                    return;

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

                    // sleep only 15 seconds during gameplay, otherwise 15 minutes
                    int sleep = changesMade ? 1000 * 15 : 1000 * 60 * 15;

                    // 3. report
                    if (LibCommon.IsAppInTestMode())
                        LibCommon.SendEmail("eric@hackerdevs.com", "Console Service Successful", "At " + DateTime.Now + ". Sleep = " + sleep, "Console Service");

                    // 4. wait                    
                    System.Threading.Thread.Sleep(sleep);
                }
                catch (Exception ex)
                {
                    try
                    {
                        _exceptionCount++;
                        string body = ex.ToString();

                        if (_exceptionCount == EXCEPTION_THRESHOLD)
                            body = "PROGRAM TERMINATED AFTER " + EXCEPTION_THRESHOLD + " ATTEMPTS<br /><br />" + body;

                        LibCommon.SendEmail("eric@hackerdevs.com", "Console Service Exception", body, "Console Service");

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
