using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace gonews
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap GO_NEWS_BMP;
        Bitmap DONG_GIAO_DIEN_NGUOI_DUNG_BMP;
        Bitmap BAN_DA_DOC_BAI_NAY_BMP;
        Bitmap QUAY_LAI_BMP;
        Bitmap NHIEM_VU_NOT_ACTIVE_BMP;
        Bitmap LUU_TIN_BMP;
        Bitmap TRUOC_BMP;
        string pathListAccount = "log//list.txt";
        string pathShotcut = "fileshotcut";
        string pathGhilog = "ghilog";
        List<string> listDevicesRunning = new List<string>();

        public MainWindow()
        {
            loadData();
            InitializeComponent();
        }

        private void loadData()
        {
            GO_NEWS_BMP = (Bitmap)Bitmap.FromFile("Data//gonews.png");
            DONG_GIAO_DIEN_NGUOI_DUNG_BMP = (Bitmap)Bitmap.FromFile("Data//donggiaodienngdung.png");
            BAN_DA_DOC_BAI_NAY_BMP = (Bitmap)Bitmap.FromFile("Data//bandadocbainay.png");
            TRUOC_BMP = (Bitmap)Bitmap.FromFile("Data//truoc.png");
            QUAY_LAI_BMP = (Bitmap)Bitmap.FromFile("Data//quaylai.png");
            LUU_TIN_BMP = (Bitmap)Bitmap.FromFile("Data//luutin.png");
            NHIEM_VU_NOT_ACTIVE_BMP = (Bitmap)Bitmap.FromFile("Data//nhiemvu.png");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsExitsFileLogin())
            {
                return;
            }
            Chay();
        }

        public void Chay()
        {
            var taiKhoanSapChay = OpenAndReadFileLogin(listDevicesRunning);
            if (taiKhoanSapChay.deviceId == "")
            {
                return;
            }
            //mở gia lap lên và chờ quét thiết bị
            GhiLog.Write(taiKhoanSapChay.deviceId, $"tài khoản chạy --- {taiKhoanSapChay.nameShotcut} --- {taiKhoanSapChay.deviceId}");
            var myProcess = new Process
            {
                StartInfo = { FileName = $"{Environment.CurrentDirectory}\\fileshotcut\\{taiKhoanSapChay.nameShotcut}" }
            };
            myProcess.Start();
            Auto(taiKhoanSapChay.deviceId, myProcess);
        }

        bool isStop = false;
        private void Auto(string deviceID, Process processName)
        {
            listDevicesRunning.Add(deviceID);
            GhiLog.Write(deviceID, $"danh sách thiết bị đang chạy {listDevicesRunning.Count()}");
            Task t = new Task(() =>
            {

                #region tìm device
                // tìm devices
                List<string> decices = KAutoHelper.ADBHelper.GetDevices();
                var soLanTimDevice = 0;
                while (soLanTimDevice < 10 && !decices.Contains(deviceID))
                {
                    decices = KAutoHelper.ADBHelper.GetDevices();
                    soLanTimDevice++;
                    Common.Delay(5);
                    GhiLog.Write(deviceID, $"Tìm thiết bị lần {soLanTimDevice}");
                }
                #endregion
                if (soLanTimDevice >= 10)
                {
                    GhiLog.Write(deviceID, $"Tìm thiết bị thất bại ---- đóng chạy thiết bị khác");
                    Task b = new Task(() => { Common.Delay(30); Chay(); });
                    b.Start();
                    processName.Kill();
                    return;
                }

                #region tim gonews sau do click nó rồi đợi 10s để load
                XoaTatCa(deviceID);
                var findGonews = WaitForFindOneBitmap(deviceID, GO_NEWS_BMP);
                if (findGonews.X == findGonews.Y && findGonews.Y == 0)
                {
                    GhiLog.Write(deviceID, $"Tìm app Gonews thất bại --- Bắt đầu tìm kiếm ảnh đóng giao diện người dùng");
                    var fileDongGiaoDienNgDung = WaitForFindOneBitmap(deviceID, DONG_GIAO_DIEN_NGUOI_DUNG_BMP);
                    if (fileDongGiaoDienNgDung.X == fileDongGiaoDienNgDung.Y && fileDongGiaoDienNgDung.Y == 0)
                    {
                        GhiLog.Write(deviceID, $"Không thấy ảnh đóng giao diện người dùng --- đi cài gonews mới");
                        CaiDatGonews();
                        XoaTatCa(deviceID);
                    }
                    else
                    {
                        GhiLog.Write(deviceID, $"Không thấy ảnh đóng giao diện người dùng --- click đóng");
                        KAutoHelper.ADBHelper.Tap(deviceID, fileDongGiaoDienNgDung.X, fileDongGiaoDienNgDung.Y);
                    }
                    GhiLog.Write(deviceID, $"Tìm app gonews lần nữa");
                    findGonews = WaitForFindOneBitmap(deviceID, GO_NEWS_BMP, 5, 10);
                    if (findGonews.X == findGonews.Y && findGonews.Y == 0)
                    {
                        GhiLog.Write(deviceID, $"sau khi cài đặt vẫn không thấy gonews --- kiểm tra lại appgonews --- update bản mới nhất");
                    }
                }
                KAutoHelper.ADBHelper.Tap(deviceID, findGonews.X, findGonews.Y);
                GhiLog.Write(deviceID, $"Tìm thấy app Gonews click vào -- đợi lòa gonews");
                #endregion

                GhiLog.Write(deviceID, $"Đi điểm danh");
                DiemDanh(deviceID);
                int demluottimbao = 0;
                while (!clickNew(deviceID) && demluottimbao < 10)
                {
                    demluottimbao++;
                    Common.Delay(10);
                }
                if (demluottimbao >= 10)
                {
                    GhiLog.Write(deviceID, $"Không tìm thấy bài viết sau 10 lần, xóa đi chạy lai");
                    Task b = new Task(() => { Common.Delay(30); Chay(); });
                    b.Start();
                    processName.Kill();
                    return;
                }
                HanhDong.TangCochu(deviceID);
                int solanlap = 0;
                var solanhoanthanh = 0;
                int soLanChayMoi = 0;
                Bitmap anhcualan2 = null;
                while (true)
                {
                    // tắt app
                    this.Dispatcher.Invoke(() =>
                    {
                        if (checkTatApp.IsChecked == true && timeoffApp.Value < DateTime.Now)
                        {
                            TatApp.HenGioTatApp();
                        }
                    });

                    if (isStop) return;

                    if (solanhoanthanh >= 1)
                    {
                        GhiLog.Write(deviceID, $"Ghi lìe log --- lần {soLanChayMoi}");
                        int solandachay = GhiFileList(deviceID);//ghifile
                        soLanChayMoi++;
                        if (solandachay >= 220 && soLanChayMoi >= 5)
                        {
                            if (isFinishCongTien(deviceID, anhcualan2))
                            {
                                // ddax max so luong ngay
                                int solandehoanthanh = GhiFileList(deviceID, "finish");//ghifile
                                GhiLog.Write(deviceID, $"Hoàn thành {deviceID} --- {solandehoanthanh} số job hôm nay");
                                Task b = new Task(() => { Common.Delay(30); Chay(); });
                                b.Start();
                                processName.Kill();
                                listDevicesRunning.Remove(deviceID);
                                return;
                            }
                            else
                            {
                                // Chua max job tiep tuc lam
                                soLanChayMoi = 0;
                            }
                        }
                        else if (soLanChayMoi == 2)
                        {
                            anhcualan2 = ChupAnhDiemSo(deviceID);
                        }
                        solanhoanthanh = 0;
                    }

                    GhiLog.Write(deviceID, $"đọc bài báo 4 lần");
                    HanhDong.readNew(deviceID); //   đọc bài báo 4 lần
                    HanhDong.readNew2(deviceID); //   đọc bài báo 4 lần
                    HanhDong.readNew2(deviceID); //   đọc bài báo 4 lần
                    HanhDong.readNew(deviceID); //   đọc bài báo 4 lần
                    var screen2 = KAutoHelper.ADBHelper.ScreenShoot(deviceID);

                    var bandadocbainayPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen2, BAN_DA_DOC_BAI_NAY_BMP);
                    if (bandadocbainayPoint == null)
                    {
                        // nếu chạy 4 lần trên chưa tìm thấy thì chạy thêm lượt nữa
                        GhiLog.Write(deviceID, $"Đọc thêm lần 5");
                        HanhDong.readNew3(deviceID);
                        screen2 = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                        bandadocbainayPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen2, BAN_DA_DOC_BAI_NAY_BMP);
                    }
                    if (bandadocbainayPoint == null)
                    {
                        // nếu chạy 5 lần trên chưa tìm thấy thì chạy thêm lượt nữa
                        GhiLog.Write(deviceID, $"Đọc thêm lần 6");
                        HanhDong.readNew3(deviceID);
                        screen2 = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                        bandadocbainayPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen2, BAN_DA_DOC_BAI_NAY_BMP);
                    }

                    if (bandadocbainayPoint != null || solanlap > 2)
                    {
                        if (solanlap > 2)
                        {
                            // check treo app
                            // trả về true là bị treo
                            GhiLog.Write(deviceID, $"Kiểm tra xem app có bị treo không");
                            if (checkTreoApp(deviceID))
                            {
                                // nếu treo tắt thiết bị chạy lại
                                GhiLog.Write(deviceID, $"App bị treo, xóa đi chạy lai");
                                Task b = new Task(() => { Common.Delay(30); Chay(); });
                                b.Start();
                                processName.Kill();
                                listDevicesRunning.Remove(deviceID);
                                return;
                            }
                        }
                        findNewAndClick(deviceID, ref solanlap, ref solanhoanthanh); // tìm bài báo và click nó
                        GhiLog.Write(deviceID, $"Tìm thâí bài báo sau {solanlap} số lần lặp");
                    }
                    solanlap++;
                }
            });
            t.Start();
            Common.Delay(1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            isStop = true;
        }


        // đợi để tìm một 1bitmat
        private (int X, int Y) WaitForFindOneBitmap(string deviceID, Bitmap bitmapIMG, int solantim1 = 5, int timeDelay = 5)
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

        private bool checkTreoApp(string deviceID)
        {
            var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            var luutinPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, LUU_TIN_BMP);
            if (luutinPoint != null)
            {
                KAutoHelper.ADBHelper.Tap(deviceID, luutinPoint.Value.X, luutinPoint.Value.Y); // click vao luu tin
                Common.Delay(1);
                var screen2 = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                var luutinPoint2 = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen2, LUU_TIN_BMP);
                if (luutinPoint2 != null)
                {
                    return true; // đang bị treo
                }
                else
                {
                    KAutoHelper.ADBHelper.Tap(deviceID, luutinPoint.Value.X, luutinPoint.Value.Y); // click vào bỏ luu tin
                    return false; // không bị treo
                }
            }
            return false;
        }

        private void CaiDatGonews()
        {
            ADBHelper.ExecuteCMD("adb install gonews.apk");
        }



        //return true là đã tìm thấy và click được
        private void findNewAndClick(string deviceID, ref int solanlap, ref int demsolanchay)
        {
            int i = 0;
            while (i < 10)
            {
                if (clickNew(deviceID))
                {
                    GhiLog.Write(deviceID, $"Tìm thấy bài báo mới tiếp theo ---- dọc bài báo mới");
                    solanlap = 0;
                    demsolanchay++;
                    return;
                }
                HanhDong.KeoXuongFind(deviceID);
                HanhDong.KeoXuongFind(deviceID);
                HanhDong.KeoXuongFind(deviceID);
                HanhDong.KeoXuongFind(deviceID);
                HanhDong.KeoXuongFind(deviceID);
                HanhDong.KeoXuongFind(deviceID);
                i++;
            }
            while (i < 12)
            {
                if (clickNew(deviceID))
                {
                    GhiLog.Write(deviceID, $"Tìm thấy bài báo mới tiếp theo ---- dọc bài báo mới");
                    solanlap = 0;
                    demsolanchay++;
                    return;
                }
                HanhDong.KeoLenFind(deviceID);
                HanhDong.KeoLenFind(deviceID);
                HanhDong.KeoLenFind(deviceID);
                i++;
            }
            // tắt app gonews và click gonews chạy lại
            GhiLog.Write(deviceID, $"Không tìn thấy bài báo mới xóa đi chạy lại ------ ");
            XoaTatCa(deviceID);
            var findGonews = WaitForFindOneBitmap(deviceID, GO_NEWS_BMP);
            KAutoHelper.ADBHelper.Tap(deviceID, findGonews.X, findGonews.Y);
            WaitForFindOneBitmap(deviceID, NHIEM_VU_NOT_ACTIVE_BMP, 5, 5);
            Common.Delay(2);
            clickNew(deviceID);
        }

        public void QuayLaiAll(string deviceID)
        {
            var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            var quaylaiPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, QUAY_LAI_BMP);
            while (quaylaiPoint != null)
            {
                KAutoHelper.ADBHelper.Tap(deviceID, quaylaiPoint.Value.X, quaylaiPoint.Value.Y);
                screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                quaylaiPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, QUAY_LAI_BMP);
            }
        }

        public void QuayLai1Lan(string deviceID)
        {
            var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            var quaylaiPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, QUAY_LAI_BMP);
            if (quaylaiPoint != null)
            {
                KAutoHelper.ADBHelper.Tap(deviceID, quaylaiPoint.Value.X, quaylaiPoint.Value.Y);
            }
        }

        // return true nếu click được
        private bool clickNew(string deviceID)
        {
            GhiLog.Write(deviceID, $"Đi tìm bài báo mới và click nó");
            var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            lock (TRUOC_BMP)
            {
                var truocPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, TRUOC_BMP);
                if (truocPoint != null)
                {
                    KAutoHelper.ADBHelper.Tap(deviceID, truocPoint.Value.X + 150, truocPoint.Value.Y);
                    return true;
                }
            }
            GhiLog.Write(deviceID, $"Không tìm thấy bài báo mới nào");
            return false;
        }

        //ghi file
        public int GhiFileList(string deviceID, string result = "notfinish")
        {
            int solanchayhomnay = 0;
            lock (pathListAccount)
            {
                string[] accounts = File.ReadAllLines(pathListAccount);
                for (int i = 0; i < accounts.Count(); i++)
                {
                    var c = accounts[i].Split('|');
                    if (c[0] == deviceID)
                    {
                        var str = c[0] + "|" + c[1] + "|" + c[2] + "|" + (Convert.ToInt32(c[3]) + 1).ToString() + "|" + result;
                        solanchayhomnay = Convert.ToInt32(c[3]) + 1;
                        accounts[i] = str;
                    }
                }
                File.WriteAllLines(pathListAccount, accounts);
            }
            return solanchayhomnay;
        }

        private Bitmap ChupAnhDiemSo(string deviceID)
        {
            KAutoHelper.ADBHelper.Tap(deviceID, 85, 780);
            Common.Delay(1);
            Bitmap BMP = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            QuayLai1Lan(deviceID);
            HanhDong.KeoXuongFind(deviceID);
            clickNew(deviceID);
            return BMP;
        }

        // chonj swich sau đó xóa tất cả các tab
        private void XoaTatCa(string deviceID)
        {
            KAutoHelper.ADBHelper.Key(deviceID, KAutoHelper.ADBKeyEvent.KEYCODE_APP_SWITCH);
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 170, 240, 800, 500);
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 170, 240, 800, 500);
            KAutoHelper.ADBHelper.Swipe(deviceID, 240, 170, 240, 800, 500);
            Common.Delay(1);
            KAutoHelper.ADBHelper.Tap(deviceID, 502, 81); // click xoa
            Common.Delay(1);
            KAutoHelper.ADBHelper.Key(deviceID, KAutoHelper.ADBKeyEvent.KEYCODE_HOME);
        }

        // điểm danh yêu cầu màn hình ban đầu vừa vào gonew chưa thao tác
        // trả về màn hình gonew chưa thao tac
        private void DiemDanh(string deviceID)
        {
            var timNhiemvu = WaitForFindOneBitmap(deviceID, NHIEM_VU_NOT_ACTIVE_BMP, 5, 5);
            KAutoHelper.ADBHelper.Tap(deviceID, timNhiemvu.X, timNhiemvu.Y); // click nhiem vu

            //KAutoHelper.ADBHelper.Tap(deviceID, 266, 900); // click nhiệm vụ
            Common.Delay(2);
            KAutoHelper.ADBHelper.Tap(deviceID, 65, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 65, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 65, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 134, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 134, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 134, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 201, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 201, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 201, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 270, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 270, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 270, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 339, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 339, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 339, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 408, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 408, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 408, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 475, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 475, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 475, 270);
            KAutoHelper.ADBHelper.Tap(deviceID, 88, 900); // click báo mới
            Common.Delay(5);
        }

        private (string nameShotcut, string deviceId, int count) OpenAndReadFileLogin(List<string> listDevicesRunning)
        {
            // tìm thằng nào có số lần chạy ít nhất, mở lên để chạy
            string[] accounts = File.ReadAllLines(pathListAccount);
            var nameShotcut = "";
            var deviceId = "";
            var count = 1000;

            for (int i = 0; i < accounts.Length; i++)
            {
                var splitAccount = accounts[i].Split('|');
                if (splitAccount.Count() == 5)
                {
                    if (!listDevicesRunning.Contains(splitAccount[0]))
                    {
                        if (Convert.ToInt32(splitAccount[3]) < count && splitAccount[4] != "finish")
                        {
                            nameShotcut = splitAccount[1];
                            deviceId = splitAccount[0];
                            count = Convert.ToInt32(splitAccount[3]);
                        }
                    }
                }
            }
            if (nameShotcut == "")
            {
                MessageBox.Show("Tất cả tài khoản hôm nay đã hoàn thành");
                return ("", "", 0);
            }
            else
            {
                return (nameShotcut, deviceId, count);
            }
        }

        private bool IsExitsFileLogin()
        {
            if (!File.Exists(pathListAccount) || File.ReadAllText(pathListAccount) == "")
            {
                MessageBox.Show("Không tồn tại file tài khoản hoặc không có tài khoản vui lòng kiểm tra");
                return false;
            }
            return true;
        }

        private void Cap_nhat_danh_sach_thiet_bi(object sender, RoutedEventArgs e)
        {
            GhiLog.Cap_nhat_danh_sach_thiet_bi(pathListAccount, pathShotcut);
        }

        private void Load_date_off(object sender, RoutedEventArgs e)
        {
            TimeSpan ts = new TimeSpan(7, 30, 0);
            timeoffApp.Value = DateTime.Now > DateTime.Now.Date + ts ? DateTime.Now.AddDays(1).Date + ts : DateTime.Now.Date + ts;
            timeoffApp.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
            timeoffApp.FormatString = "HH:mm dd/MM/yyyy";
            checkTatApp.IsChecked = true;
        }

        private bool isFinishCongTien(string deviceID, Bitmap anhcu)
        {
            var BitmapAnhmoi = ChupAnhDiemSo(deviceID);
            var truocPoint = KAutoHelper.ImageScanOpenCV.FindOutPoint(anhcu, BitmapAnhmoi);
            if (truocPoint != null)
            {
                return true;
            }
            return false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            GhiLog.ResetFileLog(pathListAccount);
            DeleteFileGhilog($"{Environment.CurrentDirectory}\\{pathGhilog}");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            TatApp.TatGiaLap();
            listDevicesRunning.Clear();
        }

        public void DeleteFileGhilog(string pathfolder)
        {
            string[] files = Directory.GetFiles(pathfolder);
            foreach (string file in files)
            {
                File.Delete(file);
                Console.WriteLine($"{file} is deleted.");
            }
        }
    }
}
