// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVEm8.CliLauncher
{
    class Updater
    {
        public static bool CheckUpdate()
        {
            JObject json;
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create("https://api.evem8.com/updates/clilauncher?v=" + Application.ProductVersion);
                webRequest.UserAgent = String.Format("{0}/{1}", Application.ProductName, Application.ProductVersion);
                webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                WebResponse response = webRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string data = reader.ReadToEnd();
                    json = JObject.Parse(data);
                }
            }
            catch
            {
                json = new JObject();
            }

            if (json["version"] != null && json["version"].ToString() != Application.ProductVersion && json["version"].ToString() != Properties.Settings.Default.lastUpdateCheck)
            {
                string version = json["version"].ToString();
                string type = (json["type"] != null) ? json["type"].ToString() : "";

                if (type == "required")
                {
                    MessageBox.Show("EVEm8 CLI launcher is out of date.\nA required update is available for download.\n\nPlease go to https://evem8.com/cli to update.", "EVEm8 CLI Launcher");
                    return true;
                }
                else
                {
                    MessageBox.Show("EVEm8 CLI launcher is out of date.\nAn optional update is available for download.\n\nPlease go to https://evem8.com/cli to update.\n\nYou will not be reminded to download this version again.", "EVEm8 CLI Launcher");

                    Properties.Settings.Default.lastUpdateCheck = (string)json["version"];
                    Properties.Settings.Default.Save();
                }
            }

            return false;
        }
    }
}
