using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckUNLOCKSWDL2.Lib
{
    class sshputty
    {
        public bool Connected = false;
        private SshConnectionInfo scInfo;

        public bool PlinkSSh(SshConnectionInfo scInfo)
        {
            this.scInfo = scInfo;
            for (int i = 0; i < 10; i++)
            {
                // AddLine("SSH Connect>>" + (i + 1));
                //scInfo.Host = "192.168.1.1";
                //scInfo.User = "hguser";
                //scInfo.Pass = "bkIt0@Uq";
                //scInfo.port = "22";                
                SSH(scInfo.Host, scInfo.User, scInfo.Pass, scInfo.port);
                while (!TimeOut())
                {
                    if (ReadData.Contains("Using username") || ReadData.Contains("enter \"y\"") || ReadData.Contains("OpenWrt:"))
                    {
                        Connected = true;
                        return true;
                    }
                }
                if (!Connected)
                    Close();

            }
            return false;

        }

        public string ReadData = "";


        DateTime dt = DateTime.Now;
        public bool TimeOut()
        {
            DateTime dt0 = DateTime.Now;
            if ((dt0 - dt).TotalMinutes > 0.4)
            {
                return true;
            }
            return false;
        }

        public void AddLine(string s)
        {
            scInfo.dr.Cells["col_Note"].Value = s;
            Console.Write(scInfo.Host + ":" + s);
        }

        public void Close()
        {
            //try
            //{
            //    p.StandardInput.WriteLine("&exit");
            //}
            //catch { }
            p.Close();
            p.Dispose();
        }

        public bool SendCmd(string cmd)
        {
            ReadData = "";
            try
            {
                p.StandardInput.WriteLine(cmd);
                dt = DateTime.Now;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SendCmdAndWait(string cmd, string exp, int timeout)
        {
            ReadData = "";
            try
            {
                p.StandardInput.WriteLine(cmd);
                Thread.Sleep(100);
                Application.DoEvents();
                dt = DateTime.Now;
                TimeSpan ts = new System.TimeSpan(0, 0, timeout * 1000);
                if (string.IsNullOrEmpty(exp))
                {
                    Thread.Sleep(ts);
                    return true;
                }
                else
                {
                    while (DateTime.Now - dt > ts)
                    {
                        if (ReadData.Contains(exp))
                        {
                            return true;
                        }
                        else
                        {
                            Thread.Sleep(500);
                            Application.DoEvents();
                        }
                    }
                    return false;
                }


            }
            catch
            {
                return false;
            }
        }
        //putty -ssh -l root -pw *** 192.168.0.101
        //Plink.exe  -l root -pw *** 192.168.0.101

        //public void SSH(string ip = "192.168.0.101", string user = "root", string psw = "***")
        //{
        //    InitProcess(@"C:\Windows\System32\cmd.exe", "", null);
        //    string path = Application.StartupPath + "";
        //    p.StandardInput.WriteLine(@"C:\Windows\plink.exe -l " + user + " -pw " + psw + " " + ip);
        //}
        public void SSH(string ip, string user, string psw, string port)
        {
            //string path = Application.StartupPath + @"\plink.exe";


            string path = @"C:\Program Files\PuTTY\plink.exe";
            if (!File.Exists(@"C:\Program Files\PuTTY\plink.exe"))
            {
                path = @"C:\Program Files (x86)\PuTTY\plink.exe";
            }
            InitProcess(path, "-l " + user + " -pw " + psw + " -P " + port + " " + ip, null);
        }

        private Process p = new Process();
        private void InitProcess(string file, string args, string dir)
        {
            try
            {
                p.StartInfo.FileName = file;
                p.StartInfo.Arguments = args;
                if (!string.IsNullOrEmpty(dir))
                {
                    p.StartInfo.WorkingDirectory = dir;
                }
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                p.Start();


                new Thread(GetStdErr).Start();
                new Thread(GetStdOut).Start();
                new Thread(SetStdIn).Start();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SetStdIn: " + e.Message);
            }
        }

        private void SetStdIn()
        {
            string line = "";
            try
            {
                StreamReader reader = new StreamReader(Console.OpenStandardInput());// p.StandardOutput;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    p.StandardInput.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SetStdIn: " + e.Message);
            }
        }
        private void GetStdOut()
        {
            string line = "";
            try
            {
                StreamReader reader = p.StandardOutput;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    Console.WriteLine(">>" + line);
                    ReadData += line + "\r\n";
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GetStdOut: " + e.Message);
            }
        }

        private void GetStdErr()
        {
            string line = "";
            try
            {
                StreamReader reader = p.StandardError;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    ReadData += line + "\r\n";
                    if (ReadData.Contains("press Return"))
                        SendCmd("n");
                    Console.WriteLine("E>" + line);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GetStdErr: " + e.Message);
            }
        }

    }
}
