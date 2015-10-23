// Copyright (c) 2015 Kali Izia
// Use of this source code is governed by the MIT license that can be found in the LICENSE file.

using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVEm8.CliLauncher
{
    /// <summary>
    /// Defines command line arguments for starting the launcher
    /// </summary>
    class Options
    {
        [Option("account", Required = true,
          HelpText = "Account name")]
        public string Account { get; set; }

        [Option("server", DefaultValue = "tq",
          HelpText = "Server name (tq, sisi, duality)")]
        public string Server { get; set; }

        [Option("settingsprofile", DefaultValue = "1",
          HelpText = "Settings profile")]
        public string Settings { get; set; }

        [Option("dx9", DefaultValue = false,
          HelpText = "Force DirectX 9")]
        public Boolean Dx9 { get; set; }

        [Option("dx11", DefaultValue = false,
          HelpText = "Force DirectX 11")]
        public Boolean Dx11 { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
