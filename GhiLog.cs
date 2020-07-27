using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gonews
{
    public static class GhiLog
    {
        public static void Write(string nameFolder, string content)
        {
            return;

            //nameFolder = nameFolder.Replace(":", "_");
            //var path = $"{Environment.CurrentDirectory}\\ghilog\\{nameFolder}.txt";

            //if (!File.Exists(path))
            //{
            //    var file = File.Create(path);
            //    file.Close();
            //}
            //var stringWrite = $"{DateTime.Now} ---- {content} \n";
            //File.AppendAllText(path, stringWrite);
        }

        public static void ResetFileLog(string pathListAccount)
        {
            // resset file
            var chuoiLuu = new List<string>();
            var ts = new TimeSpan(12, 00, 00);
            var date = (DateTime.Now.Date + ts) < DateTime.Now ? DateTime.Now.AddDays(1).Date + ts : DateTime.Now.Date + ts;
            try
            {
                string[] accounts = File.ReadAllLines(pathListAccount);
                for (int i = 0; i < accounts.Length; i++)
                {
                    string chuoi = "";
                    var splitAccount = accounts[i].Split('|');
                    if (splitAccount.Count() == 6)
                    {
                        for (int j = 0; j < splitAccount.Count(); j++)
                        {
                            if (j == 2 && splitAccount[2] != date.ToString())
                            {
                                chuoi = chuoi + date.ToString() + "|";
                            }
                            else if (j == splitAccount.Count() - 3)
                            {
                                chuoi = chuoi + (splitAccount[2] != date.ToString() ? "0" : splitAccount[j]) + "|";
                            }
                            else if (j == splitAccount.Count() - 2)
                            {
                                chuoi = chuoi + "0" + "|";
                            }
                            else if (j == splitAccount.Count() - 1)
                            {
                                chuoi = chuoi + (splitAccount[2] != date.ToString() ? "notfinish" : splitAccount[j]);
                            }
                            else
                            {
                                chuoi = chuoi + splitAccount[j] + "|";
                            }
                        }
                        chuoiLuu.Add(chuoi);
                    }
                    else
                    {
                        chuoi = chuoi + accounts[i] + "|kiểmtralạitaikhoan";
                        chuoiLuu.Add(chuoi);
                    }
                }
                File.WriteAllLines(pathListAccount, chuoiLuu);
                MessageBox.Show("Reset file log và xóa ghilog thành công");
            }
            catch (Exception)
            {
                Common.Delay(1);
                ResetFileLog(pathListAccount);
            }

        }

        public static void Cap_nhat_danh_sach_thiet_bi(string pathListAccount, string pathShotcut)
        {
            if (!File.Exists(pathListAccount))
            {
                // không có danh sách
                FileStream file = File.Create(pathListAccount);
                file.Close();
            }

            string[] filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\{pathShotcut}");
            var chuoiLuu = new List<string>();
            var noidungList = File.ReadAllText(pathListAccount);
            var ts = new TimeSpan(12, 00, 00);
            var date = (DateTime.Now.Date + ts) < DateTime.Now ? DateTime.Now.AddDays(1).Date + ts : DateTime.Now.Date + ts;
            for (int i = 0; i < filePaths.Count(); i++)
            {
                if (!noidungList.Contains(Path.GetFileName(filePaths[i])))
                {
                    var myProcess = new Process
                    {
                        StartInfo = { FileName = filePaths[i] }
                    };
                    myProcess.Start();

                    string chuoi = "";

                    var dem = 0;
                    while (dem < 20)
                    {
                        var devices = KAutoHelper.ADBHelper.GetDevices();
                        if (devices.Count > 0)
                        {
                            chuoi = devices[0] + "|" + Path.GetFileName(filePaths[i]) + "|" + date.ToString() + "|0" + "|0" + "|notfinish";
                            break;
                        }
                        Common.Delay(2);
                        dem++;
                    }
                    chuoiLuu.Add(chuoi);
                    myProcess.Kill();
                    Common.Delay(1);
                }
            }
            File.AppendAllLines(pathListAccount, chuoiLuu);
            MessageBox.Show("Tạo danh sách thiết bị thành công");
        }

        //ghi file
        public static int GhiFileList(string deviceID, string pathListAccount, string result = "notfinish", bool isthatbai = false)
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
                        c[4] = isthatbai == true ? (Convert.ToInt32(c[4]) + 1).ToString() : c[4];
                        var str = c[0] + "|" + c[1] + "|" + c[2] + "|" + (Convert.ToInt32(c[3]) + 1).ToString() + "|" + c[4] + "|" + result;
                        solanchayhomnay = Convert.ToInt32(c[3]) + 1;
                        accounts[i] = str;
                    }
                }
                File.WriteAllLines(pathListAccount, accounts);
            }
            return solanchayhomnay;
        }

        public static (string nameShotcut, string deviceId, int count) GetAccount(List<string> listDevicesRunning, string pathListAccount)
        {
            // tìm thằng nào có số lần chạy ít nhất, mở lên để chạy
            string[] accounts = File.ReadAllLines(pathListAccount);
            var nameShotcut = "";
            var deviceId = "";
            var count = 1000;

            for (int i = 0; i < accounts.Length; i++)
            {
                var splitAccount = accounts[i].Split('|');
                if (splitAccount.Count() == 6)
                {
                    if (!listDevicesRunning.Contains(splitAccount[0])
                        && Convert.ToInt32(splitAccount[3]) < count
                        && Convert.ToInt32(splitAccount[4]) < 4
                        && splitAccount[5] != "finish")
                    {
                        nameShotcut = splitAccount[1];
                        deviceId = splitAccount[0];
                        count = Convert.ToInt32(splitAccount[3]);
                        return (nameShotcut, deviceId, count);
                    }
                }
            }
            MessageBox.Show("Tất cả tài khoản hôm nay đã hoàn thành");
            return ("", "", 0);
        }
    }
}
