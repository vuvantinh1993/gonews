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
            nameFolder = nameFolder.Replace(":", "_");
            var path = $"{Environment.CurrentDirectory}\\ghilog\\{nameFolder}.txt";

            if (!File.Exists(path))
            {
                var file = File.Create(path);
                file.Close();
            }
            var stringWrite = $"{DateTime.Now} ---- {content} \n";
            File.AppendAllText(path, stringWrite);
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
                    if (splitAccount.Count() == 5)
                    {
                        for (int j = 0; j < splitAccount.Count(); j++)
                        {
                            if (j == 2 && splitAccount[2] != date.ToString())
                            {
                                chuoi = chuoi + date.ToString() + "|";
                            }
                            else if (j == splitAccount.Count() - 2)
                            {
                                chuoi = chuoi + (splitAccount[2] != date.ToString() ? "0" : splitAccount[j]) + "|";
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
                            chuoi = devices[0] + "|" + Path.GetFileName(filePaths[i]) + "|" + date.ToString() + "|0" + "|notfinish";
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
    }
}
