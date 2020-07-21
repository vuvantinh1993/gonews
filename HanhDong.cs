using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gonews
{
    public static class HanhDong
    {
        public static void readNew(string deviceID)
        {
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoLenRead(deviceID);
            KeoLenRead(deviceID);
        }

        public static void readNew2(string deviceID)
        {
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoLenRead(deviceID);
        }

        public static void readNew3(string deviceID)
        {
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoLenRead(deviceID);
            KeoLenRead(deviceID);
            KeoLenRead(deviceID);
        }


        // kéo xuống dưới
        public static void KeoXuongRead(string deviceID)
        {
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 790, 240, 150, 1100);
        }

        // kéo kên trên
        public static void KeoLenRead(string deviceID)
        {
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 150, 240, 790, 1100);
        }

        public static void KeoXuongFind(string deviceID)
        {
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 790, 240, 150, 150); // cần tăng chiều dài lên
        }

        public static void KeoLenFind(string deviceID)
        {
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 150, 240, 790, 150); // cần tăng chiều dài lên
        }

        public static void TangCochu(string deviceID)
        {
            KAutoHelper.ADBHelper.Tap(deviceID, 64, 900);
            KAutoHelper.ADBHelper.Tap(deviceID, 64, 900);
            KAutoHelper.ADBHelper.Tap(deviceID, 64, 900);
            KAutoHelper.ADBHelper.Tap(deviceID, 64, 900);
            KAutoHelper.ADBHelper.Tap(deviceID, 64, 900);
            KAutoHelper.ADBHelper.Tap(deviceID, 64, 900);
        }

    }
}
