using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
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
        Bitmap DONG_GONEWS_KHONG_PHAN_hOI;
        Bitmap BAN_DA_DOC_BAI_NAY_BMP;
        Bitmap QUAY_LAI_BMP;
        Bitmap NHIEM_VU_NOT_ACTIVE_BMP;
        Bitmap LUU_TIN_BMP;
        Bitmap TRUOC_BMP;
        Bitmap UPDATE3GACH_BMP;
        Bitmap UPDATECAPNHATATCA_BMP;
        Bitmap UPDATEKHONGCOBANCAPNHATNAO_BMP;
        public static string pathListAccount = "log//list.txt";
        public static string pathShotcut = "fileshotcut";
        public static string pathGhilog = "ghilog";
        List<string> listDevicesRunning = new List<string>();
        public static bool? ischeckGhiLog = false;


        public MainWindow()
        {
            loadData();
            InitializeComponent();
        }
        private void loadData()
        {
            GO_NEWS_BMP = (Bitmap)Bitmap.FromFile("Data//gonews.png");
            DONG_GIAO_DIEN_NGUOI_DUNG_BMP = (Bitmap)Bitmap.FromFile("Data//donggiaodienngdung.png");
            DONG_GONEWS_KHONG_PHAN_hOI = (Bitmap)Bitmap.FromFile("Data//donggiaodienngdung.png");
            BAN_DA_DOC_BAI_NAY_BMP = (Bitmap)Bitmap.FromFile("Data//bandadocbainay.png");
            TRUOC_BMP = (Bitmap)Bitmap.FromFile("Data//truoc.png");
            QUAY_LAI_BMP = (Bitmap)Bitmap.FromFile("Data//quaylai.png");
            LUU_TIN_BMP = (Bitmap)Bitmap.FromFile("Data//luutin.png");
            NHIEM_VU_NOT_ACTIVE_BMP = (Bitmap)Bitmap.FromFile("Data//nhiemvu.png");
            UPDATE3GACH_BMP = (Bitmap)Bitmap.FromFile("Data//update3gach.png");
            UPDATECAPNHATATCA_BMP = (Bitmap)Bitmap.FromFile("Data//updateCapnhatTatCa.png");
            UPDATEKHONGCOBANCAPNHATNAO_BMP = (Bitmap)Bitmap.FromFile("Data//udpateKhongCoBanCapNhatNao.png");
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
            var taiKhoanSapChay = GhiLog.GetAccount(listDevicesRunning, pathListAccount);
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
            Thread t = new Thread(() =>
            {

                #region tìm device
                // tìm devices
                List<string> decices = KAutoHelper.ADBHelper.GetDevices();
                var soLanTimDevice = 0;
                while (soLanTimDevice < 30 && !decices.Contains(deviceID))
                {
                    decices = KAutoHelper.ADBHelper.GetDevices();
                    soLanTimDevice++;
                    Common.Delay(2);
                    GhiLog.Write(deviceID, $"Tìm thiết bị lần {soLanTimDevice}");
                }
                if (soLanTimDevice >= 30)
                {
                    GhiLog.Write(deviceID, $"Tìm thiết bị thất bại ---- đóng chạy lại");
                    GhiLog.GhiFileList(deviceID, pathListAccount, "notfinish", true);
                    TatGiaLapvaChayLai(deviceID, processName);
                    return;
                }
                #endregion

                #region tim gonews sau do click
                var findGonews = HanhDong.WaitForFindOneBitmap(deviceID, GO_NEWS_BMP, 10, 2);
                if (findGonews.X == findGonews.Y && findGonews.Y == 0)
                {
                    HanhDong.XoaTatCa(deviceID);
                    GhiLog.Write(deviceID, $"Tìm app Gonews thất bại --- Bắt đầu tìm kiếm ảnh đóng giao diện người dùng");
                    var fileDongGiaoDienNgDung = HanhDong.WaitForFindOneBitmap(deviceID, DONG_GIAO_DIEN_NGUOI_DUNG_BMP, 1, 1);
                    if (fileDongGiaoDienNgDung.X == fileDongGiaoDienNgDung.Y && fileDongGiaoDienNgDung.Y == 0)
                    {
                        GhiLog.Write(deviceID, $"Không thấy ảnh đóng giao diện người dùng --- bỏ qua");
                    }
                    else
                    {
                        GhiLog.Write(deviceID, $"Tìm thấy ảnh đóng giao diện người dùng --- click đóng");
                        KAutoHelper.ADBHelper.Tap(deviceID, fileDongGiaoDienNgDung.X, fileDongGiaoDienNgDung.Y);
                    }
                    GhiLog.Write(deviceID, $"Tìm app gonews lần nữa");
                    findGonews = HanhDong.WaitForFindOneBitmap(deviceID, GO_NEWS_BMP, 10, 2);
                    if (findGonews.X + findGonews.Y == 0)
                    {
                        GhiLog.Write(deviceID, $"sau khi cài đặt vẫn không thấy gonews --- kiểm tra lại appgonews --- update bản mới nhất --- bỏ qua thiết bị này");
                        GhiLog.GhiFileList(deviceID, pathListAccount, "notfinish", true);
                        TatGiaLapvaChayLai(deviceID, processName);
                        return;
                    }
                }

                #region update gonews bản mới nhất
                bool? isCheckUpdateversion = false;
                this.checkUpdateVersion.Dispatcher.Invoke(() =>
                {
                    isCheckUpdateversion = checkUpdateVersion.IsChecked;
                });


                this.checkGhiLog.Dispatcher.Invoke(() =>
                {
                    ischeckGhiLog = checkGhiLog.IsChecked;
                });

                if (isCheckUpdateversion == true)
                {
                    GhiLog.Write(deviceID, $"Yêu cầu update gonews bản mới nhất");
                    HanhDong.UpdateGonews(deviceID, UPDATE3GACH_BMP, UPDATECAPNHATATCA_BMP, UPDATEKHONGCOBANCAPNHATNAO_BMP);
                    HanhDong.XoaTatCa(deviceID);
                }

                #endregion

                KAutoHelper.ADBHelper.Tap(deviceID, findGonews.X, findGonews.Y);
                GhiLog.Write(deviceID, $"Tìm thấy app Gonews click vào -- đợi lòa gonews");
                #endregion

                GhiLog.Write(deviceID, $"Đi điểm danh");
                DiemDanh(deviceID);
                GhiLog.Write(deviceID, $"điểm danh xong");
            GOTO_MO_LAI_GO_NEW:
                int demluottimbao = 0;
                while (!clickNew(deviceID) && demluottimbao < 30)
                {
                    demluottimbao++;
                    Common.Delay(2);
                }
                if (demluottimbao >= 20)
                {
                    #region Kiểm tra gonews có bị treo app haytự đóng ứng dụng không nếu có đóng ứng dụng bật lại
                    var timtreoapp = HanhDong.WaitForFindTwoBitmap(deviceID, DONG_GIAO_DIEN_NGUOI_DUNG_BMP, GO_NEWS_BMP, 1, 1);
                    if (timtreoapp.sttBitmap == 0)
                    {
                        // không tìm thấy bỏ qua
                    }
                    else if (timtreoapp.sttBitmap == 1)
                    {
                        KAutoHelper.ADBHelper.Tap(deviceID, timtreoapp.X, timtreoapp.Y);
                        if (ClickAppGonews(deviceID))
                        {
                            goto GOTO_MO_LAI_GO_NEW;
                        }
                    }
                    else
                    {
                        if (ClickAppGonews(deviceID))
                        {
                            goto GOTO_MO_LAI_GO_NEW;
                        }
                    }
                    #endregion

                    GhiLog.Write(deviceID, $"Không tìm thấy bài viết sau 30 lần -- 2s/lần, xóa đi chạy lai");
                    GhiLog.GhiFileList(deviceID, pathListAccount, "notfinish", true);
                    TatGiaLapvaChayLai(deviceID, processName);
                    return;
                }
                HanhDong.TangCochu(deviceID);
                int solanlap = 0;
                var solanhoanthanh = 0;
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
                        int solandachay = GhiLog.GhiFileList(deviceID, pathListAccount); //ghifile

                        int soJobMaxTrongNgay = 300;
                        this.soJobTrongNgay.Dispatcher.Invoke(() =>
                        {
                            soJobMaxTrongNgay = Convert.ToInt32(soJobTrongNgay.Text);
                        });

                        if (solandachay >= soJobMaxTrongNgay)
                        {
                            // ddax max so luong ngay
                            GhiLog.Write(deviceID, $"Hoàn thành số job của {deviceID} --- {solandachay} đã chạy hôm nay");
                            GhiLog.GhiFileList(deviceID, pathListAccount, "finish"); //ghifile
                            TatGiaLapvaChayLai(deviceID, processName);
                            return;
                        }
                    }


                    GhiLog.Write(deviceID, $"đọc bài báo 4 lần");
                    HanhDong.readNew(deviceID); //   đọc bài báo 4 lần
                    HanhDong.readNew2(deviceID); //   đọc bài báo 4 lần
                    HanhDong.readNew2(deviceID); //   đọc bài báo 4 lần
                    HanhDong.readNew4(deviceID); //   đọc bài báo 4 lần
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

                    GhiLog.Write(deviceID, $"---------------solanlap -------------{solanlap}");
                    if (bandadocbainayPoint != null || solanlap > 1)
                    {
                        if (solanlap > 1)
                        {
                            #region Kiểm tra gonews có bị treo app haytự đóng ứng dụng không nếu có đóng ứng dụng bật lại
                            var timtreoapp = HanhDong.WaitForFindTwoBitmap(deviceID, DONG_GIAO_DIEN_NGUOI_DUNG_BMP, GO_NEWS_BMP, 1, 1);
                            if (timtreoapp.sttBitmap == 0)
                            {
                                // không tìm thấy bỏ qua
                            }
                            else if (timtreoapp.sttBitmap == 1)
                            {
                                KAutoHelper.ADBHelper.Tap(deviceID, timtreoapp.X, timtreoapp.Y);
                                if (ClickAppGonews(deviceID))
                                {
                                    Common.Delay(7);
                                    goto GOTO_TIMBAIBAOVACLICKNO;
                                }
                            }
                            else
                            {
                                if (ClickAppGonews(deviceID))
                                {
                                    Common.Delay(7);
                                    goto GOTO_TIMBAIBAOVACLICKNO;
                                }
                            }
                            #endregion


                            #region check treo hoàn toàn giả lập không thao tác được gì
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
                            #endregion
                        }
                    GOTO_TIMBAIBAOVACLICKNO:
                        if (!findNewAndClick(deviceID, ref solanlap, ref solanhoanthanh, processName))
                        {
                            return;
                        }
                        GhiLog.Write(deviceID, $"Tìm thâí bài báo sau {solanlap} số lần lặp");
                    }
                    solanlap++;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            Common.Delay(1);
        }

        private void TatGiaLapvaChayLai(string deviceID, Process processName)
        {
            listDevicesRunning.Remove(deviceID);
            Task b = new Task(() => { Common.Delay(30); Chay(); });
            b.Start();
            processName.Kill();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            isStop = true;
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


        private bool ClickAppGonews(string deviceID)
        {
            var findGonews = HanhDong.WaitForFindOneBitmap(deviceID, GO_NEWS_BMP, 10, 2);
            if (findGonews.X + findGonews.Y != 0)
            {
                KAutoHelper.ADBHelper.Tap(deviceID, findGonews.X, findGonews.Y);
                return true;
            }
            return false;
        }


        //return true là đã tìm thấy và click được
        private bool findNewAndClick(string deviceID, ref int solanlap, ref int demsolanchay, Process processName)
        {
            int i = 0;
            while (i < 10)
            {
                if (clickNew(deviceID))
                {
                    GhiLog.Write(deviceID, $"Tìm thấy bài báo mới tiếp theo ---- dọc bài báo mới");
                    solanlap = 0;
                    demsolanchay++;
                    return true;
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
                    return true;
                }
                HanhDong.KeoLenFind(deviceID);
                HanhDong.KeoLenFind(deviceID);
                HanhDong.KeoLenFind(deviceID);
                i++;
            }
            // tắt app gonews và click gonews chạy lại
            GhiLog.Write(deviceID, $"Không tìn thấy bài báo mới tắt gải lập đi chạy lại ------ ");
            GhiLog.GhiFileList(deviceID, pathListAccount, "notfinish", true);
            TatGiaLapvaChayLai(deviceID, processName);
            return false;
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
                GhiLog.Write(deviceID, $"Đang khóa ảnh trước của bài báo");
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



        // điểm danh yêu cầu màn hình ban đầu vừa vào gonew chưa thao tác
        // trả về màn hình gonew chưa thao tac
        private void DiemDanh(string deviceID)
        {
            var timNhiemvu = HanhDong.WaitForFindOneBitmap(deviceID, NHIEM_VU_NOT_ACTIVE_BMP, 20, 2);
            if (timNhiemvu.X + timNhiemvu.Y != 0)
            {
                KAutoHelper.ADBHelper.Tap(deviceID, timNhiemvu.X, timNhiemvu.Y); // click nhiem vu
                Common.Delay(2);
                KAutoHelper.ADBHelper.Tap(deviceID, 65, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 65, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 134, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 134, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 201, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 201, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 270, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 270, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 339, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 339, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 408, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 408, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 475, 270);
                KAutoHelper.ADBHelper.Tap(deviceID, 475, 270);
            }
            KAutoHelper.ADBHelper.Tap(deviceID, 88, 900); // click báo mới
            Common.Delay(1);
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
            TimeSpan ts = new TimeSpan(8, 00, 0);
            timeoffApp.Value = DateTime.Now > DateTime.Now.Date + ts ? DateTime.Now.AddDays(1).Date + ts : DateTime.Now.Date + ts;
            timeoffApp.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
            timeoffApp.FormatString = "HH:mm dd/MM/yyyy";
            if (timeoffApp.Value?.DayOfWeek != DayOfWeek.Saturday || timeoffApp.Value?.DayOfWeek != DayOfWeek.Sunday)
            {
                checkTatApp.IsChecked = true;
            }
            checkUpdateVersion.IsChecked = false;
            checkGhiLog.IsChecked = false;
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

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Process.Start($"{Environment.CurrentDirectory}\\{pathListAccount}");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var fullPath = $"{Environment.CurrentDirectory}\\{pathGhilog}";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.GetFileName(fullPath);
            Process.Start(psi);
        }
    }
}
