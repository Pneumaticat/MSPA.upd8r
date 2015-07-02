using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Configuration;
using MSPA.upd8r.Properties;

namespace MSPA.upd8r
{
    class Program
    {
        static Options CommandLineOptions = new Options();

        private static bool ExistsAtAddress(string url)
        {
            WebRequest req = HttpWebRequest.Create(url);
            req.Method = "HEAD";
            WebResponse res = null;

            try
            {
                res = req.GetResponse();
                res.Close();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (res != null)
                    res.Close();
            }
        }

        private static bool HSPageExists(int number, int tries = 3)
        {
            int remainingTries = tries;
            while (remainingTries != 0)
            {
                if (ExistsAtAddress(String.Format("http://www.mspaintadventures.com/6/{0:d6}.txt", number)))
                    return true;
                else
                {
                    remainingTries = remainingTries - 1;
                    Thread.Sleep(100);
                }
            }

            // Tries have run out. Page most certainly does not exist.
            return false;
        }

        static void Main(string[] args)
        {
            Console.Title = "MSPA.upd8r";

            CommandLine.Parser.Default.ParseArguments(args, CommandLineOptions);

            Console.WriteLine("Updating latest page marker...");
            Console.WriteLine();

            int savedLatestPage = Settings.Default.SavedLatestPage;
            int latestPage = UpdateLatestPage(savedLatestPage);

            Console.WriteLine();
            Console.WriteLine("Checking for new pages about every 10s...");
            Console.WriteLine();

            while(true)
            {
                PageCheckResults results = CheckForNewPage(ref latestPage);
                if (results.newPageExists)
                {
                    // A new page exists. Note that latestPage is now changed to point to the new
                    // latest page by CheckForNewPage, ensuring that CheckForNewPage on the next
                    // loop has the right number to check from. (did that make any sense at all?
                    // hopefully)
                    Console.WriteLine("[{0}] New page exists! Number: {1}", DateTime.Now, results.page.number);
                    Console.WriteLine("[{0}] latestPage is now at {1}", DateTime.Now, latestPage);
                    UpdateNewPageJson(results);
                }
                else
                    Console.WriteLine("[{0}] No new page found. Current latest page is {1}", DateTime.Now, latestPage);

                Thread.Sleep(10000);
            }
        }

        static void UpdateNewPageJson(PageCheckResults page)
        {
            string filename;
            if (CommandLineOptions.JsonFile != null)
                filename = CommandLineOptions.JsonFile;
            else
                filename = "new-page.json";

            string json = JsonConvert.SerializeObject(page, Formatting.Indented);
            File.WriteAllText(filename, json, Encoding.UTF8);
        }

        /// <summary>
        /// Gets page information - title and number.
        /// Note: this method DOES NOT give up. Just as a
        /// warning.
        /// </summary>
        /// <param name="number">Number of a Homestuck page.</param>
        /// <returns></returns>
        static PageInformation GetPageInformation(int number)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;

                string pageTxt;
                while (true)
                {
                    try
                    {
                        pageTxt = client.DownloadString(String.Format("http://www.mspaintadventures.com/6/{0:d6}.txt", number));
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Download of page failed, trying again...");
                        Thread.Sleep(100);
                    }
                }

                string firstLine = new string(pageTxt.TakeWhile(c => c != '\n').ToArray());

                return new PageInformation
                {
                    command = firstLine,
                    number = number
                };
            }
        }

        /// <summary>
        /// Checks to see if there is a new page of Homestuck.
        /// If there is, gets the new latest page and changes the
        /// latestPage parameter to match.
        /// </summary>
        /// <param name="latestPage">
        /// Passed in: the current latest page. If there is a new
        /// page from the current latest page, this is changed to
        /// be the new latest page.
        /// </param>
        /// <returns>
        /// Information on the new page, and whether or not there
        /// is a new page.
        /// </returns>
        static PageCheckResults CheckForNewPage(ref int latestPage)
        {
            PageCheckResults result;
            if (HSPageExists(latestPage + 1))
                result = new PageCheckResults { newPageExists = true, page = GetPageInformation(latestPage + 1) };
            else if (HSPageExists(latestPage + 2))
                result = new PageCheckResults { newPageExists = true, page = GetPageInformation(latestPage + 2) };
            else
                result = new PageCheckResults { newPageExists = false };

            if (result.newPageExists)
            {
                Console.WriteLine("Page found. Updating latestPage...");
                latestPage = UpdateLatestPage(beginningTestPage: latestPage);
                // number of pages: new latest page - found new page number + 1
                result.numberOfPages = latestPage - result.page.number + 1;
            }

            return result;
        }

        static int UpdateLatestPage(int beginningTestPage)
        {
            int testPage = beginningTestPage;
            while (HSPageExists(testPage)
                || HSPageExists(testPage + 1))
            {
                Console.WriteLine("=> Testing page {0}", testPage);
                testPage++;
            }

            int latestPage = testPage - 1;

            Console.WriteLine("Latest page marker is now at {0}", latestPage);

            // Save LatestPage to file to be read upon startup next time
            Settings.Default.SavedLatestPage = latestPage;
            Settings.Default.Save();

            return latestPage;
        }
    }
}
