using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gonews
{
    public static class HanhDong
    {
        public static void readNew(string deviceID)
        {
            Random r = new Random();
            var numberrandom = r.Next(2, 5);
            switch (numberrandom)
            {
                case 2:
                    KeoXuongRead(deviceID);
                    KeoLenRead(deviceID);
                    break;
                case 3:
                    KeoXuongRead(deviceID);
                    KeoXuongRead(deviceID);
                    KeoLenRead(deviceID);
                    break;
                case 4:
                    KeoXuongRead(deviceID);
                    KeoXuongRead(deviceID);
                    KeoXuongRead(deviceID);
                    KeoLenRead(deviceID);
                    break;
                default:
                    KeoXuongRead(deviceID);
                    KeoXuongRead(deviceID);
                    KeoXuongRead(deviceID);
                    KeoXuongRead(deviceID);
                    KeoLenRead(deviceID);
                    break;
            }
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

        public static void readNew4(string deviceID)
        {

            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoXuongRead(deviceID);
            KeoLenRead(deviceID);
            KeoLenRead(deviceID);
        }

        public static void UpdateGonews(string deviceID, Bitmap Updatecapnhat3gach, Bitmap Updatecapnhattatca, Bitmap Updatekhongcobancapnhatnao)
        {
            KAutoHelper.ADBHelper.Tap(deviceID, 60, 180);
            Common.DelayMiliSecond(500);
            KAutoHelper.ADBHelper.Tap(deviceID, 290, 320);
            Common.DelayMiliSecond(500);
            var find3gach = WaitForFindOneBitmap(deviceID, Updatecapnhat3gach, 10, 1);
            if ((find3gach.X + find3gach.Y) == 0)
            {
                GhiLog.Write(deviceID, $"Update --- Không thấy buttom 3 gạch, tap vào màn hình");
                KAutoHelper.ADBHelper.Tap(deviceID, 76, 78);
            }
            else
            {
                GhiLog.Write(deviceID, $"Update --- Tìm thấy buttom 3 gạch, tap vào màn hình");
                KAutoHelper.ADBHelper.Tap(deviceID, find3gach.X, find3gach.Y);
            }
            Common.DelayMiliSecond(200);
            KAutoHelper.ADBHelper.Tap(deviceID, 130, 170);
            GhiLog.Write(deviceID, $"Update --- tìm cap nhat tat cả hoặc không cớ cập nhật nào");
            var find2Bitmap = WaitForFindTwoBitmap(deviceID, Updatecapnhattatca, Updatekhongcobancapnhatnao, 10, 1);
            if (find2Bitmap.sttBitmap == 0)
            {
                GhiLog.Write(deviceID, $"Update --- Lỗi............. update không thành công kiểm tra lại");
            }
            else if (find2Bitmap.sttBitmap == 1)
            {
                GhiLog.Write(deviceID, $"Update --- tìm thấy button cập nhật tất cả");
                KAutoHelper.ADBHelper.Tap(deviceID, find2Bitmap.X, find2Bitmap.Y);
                var find = WaitForFindOneBitmap(deviceID, Updatekhongcobancapnhatnao, 20, 2);
                if (find.X + find.Y != 0)
                {
                    GhiLog.Write(deviceID, $"Update --- tìm thấy button không có bản cập nhật nào --- cập nhật thành công");
                }
                else
                {
                    GhiLog.Write(deviceID, $"Update --- Lỗi............. update không thành công kiểm tra lại");
                }
            }
            else
            {
                GhiLog.Write(deviceID, $"Update --- tìm thấy button không có bản cập nhật nào --- cập nhật thành công");
            }
        }

        // đợi để tìm một 1bitmat
        public static (int X, int Y) WaitForFindOneBitmap(string deviceID, Bitmap bitmapIMG, int solantim1 = 5, int timeDelay = 5)
        {
            var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            var imgFind = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, bitmapIMG);
            var solantim = 0;
            while (solantim < solantim1 && imgFind == null)
            {
                Common.Delay(timeDelay);
                screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                imgFind = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, bitmapIMG);
                solantim++;
            }
            if (imgFind != null)
            {
                return (imgFind.Value.X, imgFind.Value.Y);
            }
            return (0, 0);
        }

        // đợi để 1 trong 2 bitmap đưa vào nếu thấy cqái nào trả cái đó
        // mặc định là không tìm thấy nếu thấy 2 cái trả về cái đầu tiên
        public static (int sttBitmap, int X, int Y) WaitForFindTwoBitmap(string deviceID, Bitmap bitmapIMG1, Bitmap bitmapIMG2, int solantim1 = 5, int timeDelay = 5)
        {
            var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            var imgFind1 = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, bitmapIMG1);
            var imgFind2 = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, bitmapIMG2);

            var solantim = 0;
            while (solantim < solantim1 && (imgFind1 == null && imgFind2 == null))
            {
                Common.Delay(timeDelay);
                screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                imgFind1 = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, bitmapIMG1);
                imgFind2 = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, bitmapIMG2);
                solantim++;
            }
            if (imgFind1 != null)
            {
                return (1, imgFind1.Value.X, imgFind1.Value.Y);
            }
            else if (imgFind2 != null)
            {
                return (2, imgFind2.Value.X, imgFind2.Value.Y);
            }
            else
            {
                return (0, 0, 0);
            }
        }


        // chonj swich sau đó xóa tất cả các tab
        public static void XoaTatCa(string deviceID)
        {
            KAutoHelper.ADBHelper.Key(deviceID, KAutoHelper.ADBKeyEvent.KEYCODE_APP_SWITCH);
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 170, 240, 800, 250);
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 170, 240, 800, 250);
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 170, 240, 800, 250);
            Common.DelayMiliSecond(300);
            KAutoHelper.ADBHelper.Tap(deviceID, 502, 81); // click xoa
            Common.DelayMiliSecond(300);
            KAutoHelper.ADBHelper.Key(deviceID, KAutoHelper.ADBKeyEvent.KEYCODE_HOME);
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
