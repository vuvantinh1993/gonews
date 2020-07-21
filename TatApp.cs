using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gonews
{
    public static class TatApp
    {
        public static void HenGioTatApp()
        {
            TatGiaLap();
            TatGonews();
        }

        public static void TatGiaLap()
        {
            foreach (var process in Process.GetProcessesByName("dnplayer"))
            {
                process.Kill();
            }
        }

        public static void TatGonews()
        {
            foreach (var process in Process.GetProcessesByName("gonews"))
            {
                process.Kill();
            }
        }
    }
}
