// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace EVEm8.CliLauncher
{
    /// <summary>
    /// Functions for working with the new EVE launcher (bootstrapper)
    /// </summary>
    class EveBootstrapper
    {
        private static string baseUrl = "http://d17ueqc3zm9j8o.cloudfront.net";
        private static string versionUrl = "{0}/evelauncher_release.json";
        private static string indexUrl = "{0}/evelauncher_{1}.txt";

        // FIXME: Temporary
        public static Dictionary<string, string> servers = new Dictionary<string, string>
        {
            {"tq", "87.237.38.200"},
            {"sisi", "87.237.38.50"},
            {"duality", "87.237.38.60"},
            {"mp", "87.237.38.51"},
            {"chaos", "87.237.38.55"},
        };
        private static Dictionary<string, string> refreshUrls = new Dictionary<string,string>
        {
            {"tq", "https://client.eveonline.com/launcher/en/SSORefreshUser/"},
            {"sisi", "https://client.testeveonline.com/launcher/en/SSORefreshUser/"},
            {"duality", "https://dualityclient.testeveonline.com/launcher/en/SSORefreshUser/"},
            {"mp", "https://multiclient.eveonline.com/launcher/en/SSORefreshUser/"},
            {"chaos", "https://chaosclient.eveonline.com/launcher/en/SSORefreshUser/"},
        };
        private static Dictionary<string, string> loginUrls = new Dictionary<string, string>
        {
            {"tq", "https://login.eveonline.com/launcher/token?accesstoken={0}"},
            {"sisi", "https://sisilogin.testeveonline.com/launcher/token?accesstoken={0}"},
            {"duality", "https://dualitylogin.testeveonline.com/launcher/token?accesstoken={0}"},
            {"mp", "https://multilogin.testeveonline.com/launcher/token?accesstoken={0}"},
            {"chaos", "https://chaoslogin.testeveonline.com/launcher/token?accesstoken={0}"},
        };

        /// <summary>
        /// Get current release version of the EVE launcher
        /// </summary>
        /// <returns>Version number</returns>
        public static int GetVersion()
        {
            try
            {
                JObject json;
                var webRequest = (HttpWebRequest)WebRequest.Create(String.Format(EveBootstrapper.versionUrl, EveBootstrapper.baseUrl));
                webRequest.UserAgent = String.Format("{0}/{1}", Application.ProductName, Application.ProductVersion);
                webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                WebResponse response = webRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string data = reader.ReadToEnd();
                    json = JObject.Parse(data);
                    if (json["version"] != null)
                    {
                        return (int)json["version"];
                    }
                }
            }
            catch (Exception e)
            {
                throw new VersionException(e);
            }

            return 0;
        }

        /// <summary>
        /// Get resfile index for the specified launcher version
        /// </summary>
        /// <param name="version">Version number</param>
        /// <returns>Dict</returns>
        public static Dictionary<string, ResfileIndexEntry> GetIndex(int version)
        {
            var index = new Dictionary<string, ResfileIndexEntry>();

            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(String.Format(EveBootstrapper.indexUrl, EveBootstrapper.baseUrl, version));
                webRequest.UserAgent = String.Format("{0}/{1}", Application.ProductName, Application.ProductVersion);
                webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                WebResponse response = webRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string data = reader.ReadToEnd();

                    string[] rows = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (var row in rows)
                    {
                        string[] cols = row.Split(new string[] { "," }, StringSplitOptions.None);
                        if (cols.Length == 5)
                        {
                            ResfileIndexEntry entry = new ResfileIndexEntry(cols[0], cols[1], cols[2], Int32.Parse(cols[3]), Int32.Parse(cols[4]));
                            index.Add(cols[0], entry);
                        }                       
                    }
                }
            }
            catch (Exception e)
            {
                throw new IndexException(e);
            }

            return index;
        }

        /// <summary>
        /// Loads a resource from the CDN as a JSON object
        /// </summary>
        /// <param name="fileName">Cached filename</param>
        /// <returns>JObject</returns>
        public static JObject GetJsonResource(string fileName)
        {
            JObject json;

            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(String.Format("{0}/{1}", EveBootstrapper.baseUrl, fileName));
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
            catch (Exception e)
            {
                throw new ResourceException(e);
            }

            return json;
        }

        /// <summary>
        /// Get all refresh tokens for the specified server
        /// </summary>
        /// <param name="server">Server name</param>
        /// <returns>Dict</returns>
        public static Dictionary<string, string> GetTokens(string server)
        {
            var tokens = new Dictionary<string, string>();

            string baseDir = String.Format(@"{0}\CCP\EVEONLINE\QtWebEngine\Default\Local Storage\",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            string[] files = Directory.GetFiles(baseDir, "*.localstorage");
            foreach (var file in files)
            {
                SQLiteConnection dbConnection = new SQLiteConnection(String.Format("Data Source={0};Version=3;", file));
                dbConnection.Open();

                SQLiteCommand command = dbConnection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT value FROM itemTable WHERE key LIKE @key";
                command.Parameters.Add(new SQLiteParameter("@key", String.Format("tokens{0}_%", server)));
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    byte[] blob = (byte[])reader["value"];
                    string value = Encoding.Unicode.GetString(blob, 0, blob.Length);
                    JObject json = JObject.Parse(value);
                    foreach (var item in json)
                    {
                        if (item.Value["userName"] != null && item.Value["refreshToken"] != null)
                        {
                            if (!tokens.ContainsKey((string)item.Value["userName"]))
                            {
                                tokens.Add((string)item.Value["userName"], (string)item.Value["refreshToken"]);
                            }
                        }
                    }
                }

                dbConnection.Close();
            }

            return tokens;
        }

        /// <summary>
        /// Logs in using the refresh token and returns a launcher token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>Launcher token</returns>
        public static string GetLauncherToken(string server, string refreshToken)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(String.Empty);
            query.Add("refreshToken", refreshToken);
            var postData = Encoding.ASCII.GetBytes(query.ToString());

            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(EveBootstrapper.refreshUrls[server]);
                webRequest.UserAgent = String.Format("{0}/{1}", Application.ProductName, Application.ProductVersion);
                webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = postData.Length;

                using (var request = webRequest.GetRequestStream())
                {
                    request.Write(postData, 0, postData.Length);
                }

                WebResponse response = webRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string data = reader.ReadToEnd();
                    JObject json = JObject.Parse(data);
                    if (json["AccessToken"] != null)
                    {
                        return (string)json["AccessToken"];
                    }
                }
            }
            catch (Exception e)
            {
                throw new TokenException(e);
            }

            throw new TokenException(new Exception("Failed to get launcher token"));
        }

        /// <summary>
        /// Logs in using the launcher token and returns an access token
        /// </summary>
        /// <param name="launcherToken">Launcher token</param>
        /// <returns>Launcher token</returns>
        public static string GetAccessToken(string server, string launcherToken)
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(String.Format(EveBootstrapper.loginUrls[server], launcherToken));
                webRequest.UserAgent = String.Format("{0}/{1}", Application.ProductName, Application.ProductVersion);
                webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                webRequest.AllowAutoRedirect = false;

                WebResponse response = webRequest.GetResponse();
                if (response.Headers["Location"] != null)
                {
                    Regex regex = new Regex(@"#access_token=(.*?)&");
                    Match match = regex.Match(response.Headers["Location"]);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            catch (Exception e)
            {
                throw new TokenException(e);
            }

            throw new TokenException(new Exception("Failed to get access token"));
        }

        /// <summary>
        /// Get the client's folder location for the specified server
        /// </summary>
        /// <param name="server">Server name</param>
        /// <returns>Folder as a string</returns>
        public static string GetClientLocation(string server)
        {
            string cacheFolder = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\CCP\EVEONLINE", "CACHEFOLDER", null);
            if (cacheFolder != null)
            {
                return cacheFolder + "\\" + server;
            }

            return @"C:\EVE\SharedCache\" + server;
        }
    }
}
