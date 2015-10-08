// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEm8.CliLauncher
{
    class Launcher
    {
        /// <summary>
        /// Attemps to launch an account defined in the EVE launcher
        /// </summary>
        /// <param name="accountName">The account name defined in the EVE launcher</param>
        /// <param name="error">Returns a string describing the error if function returns False</param>
        /// <returns>True if account was launched successfully, or False if there were any errors</returns>
        public static Boolean LaunchAccount(Options options, out string error)
        {
            error = null;

            // Load server list
            var servers = EveBootstrapper.servers;

            if (!servers.ContainsKey(options.Server))
            {
                error = "Invalid server selected: " + options.Server;
                return false;
            }

            // Get tokens for selected server
            var access_token = "";
            var tokens = EveBootstrapper.GetTokens(options.Server);
            if (!tokens.ContainsKey(options.Account))
            {
                error = "No tokens found for account '" + options.Account  + "' on " + options.Server;
                return false;
            }
            else
            {
                try
                {
                    var launcher_token = EveBootstrapper.GetLauncherToken(options.Server, tokens[options.Account]);
                    access_token = EveBootstrapper.GetAccessToken(options.Server, launcher_token);
                }
                catch (TokenException e)
                {
                    error = "Token for '" + options.Account + "' on " + options.Server + " is invalid";
                    return false;
                }
            }

            // Get client location
            var dir = EveBootstrapper.GetClientLocation(options.Server);
            if (!Directory.Exists(dir))
            {
                error = "Unable to find an EVE install at " + dir;
                return false;
            }
            if (!File.Exists(dir + @"\bin\exefile.exe"))
            {
                error = "EVE install appears to be incomplete at " + dir;
                return false;
            }

            // Build launch string
            string launchPath = @"{0}\bin\exefile.exe";
            string launchArgs = "/noconsole /server:{0} /ssoToken={1}";
            if (options.Dx11)
            {
                launchArgs += " /triPlatform=dx11";
            }
            else if (options.Dx9)
            {
                launchArgs += " /triPlatform=dx9";
            }
            if (options.Settings != 0)
            {
                launchArgs += " /settingsprofile=" + options.Settings;
            }

            // Launch the client
            var procInfo = new ProcessStartInfo(String.Format(launchPath, dir), String.Format(launchArgs, servers[options.Server], access_token));
            procInfo.WorkingDirectory = dir;
            var proc = Process.Start(procInfo);

            // We never get here
            if (proc != null)
            {
                return true;
            }
            error = "Failed to launch exefile";
            return false;
        }
    }
}
