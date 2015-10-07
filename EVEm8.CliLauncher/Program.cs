// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVEm8.CliLauncher
{
    class Program
    {
        /// <summary>
        /// Main entry point
        /// Parses command line arguments and passes the account off to CliLauncher
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (Properties.Settings.Default.newInstall)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.newInstall = false;
                Properties.Settings.Default.Save();
            }

            if (!Updater.CheckUpdate())
            {
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    string error;
                    if (!Launcher.LaunchAccount(options, out error))
                    {
                        MessageBox.Show(error, "EVEm8 CLI Launcher");
                    }
                }
                else
                {
                    MessageBox.Show(options.GetUsage(), "EVEm8 CLI Launcher");
                }
            }
        }
    }
}
