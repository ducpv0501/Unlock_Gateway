using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckUNLOCKSWDL2.Lib
{


    internal class LibCommand
    {
        string Path;
        Logger Logger = new Logger();

        SSHPuttyHelper Putty = new SSHPuttyHelper();
        public LibCommand(string _path)
        {
            this.Path = _path;
        }
        public bool SSHConnect(string ip)
        {
           
            Logger.WriteLog(Path, "SSH PUTTY connect: " + ip);

            if (Putty.G5_SSH_connect_dut_putty(ip, "root", "adminadminadmina", "22"))
            {
                Logger.WriteLog(Path, "SSH Login Success: " + ip);
                return true;
            }
            Logger.WriteLog(Path, "SSH Login Failure: " + ip);
            Console.WriteLine("Login Fail");
            return false;

        }
        public bool CheckRunningSoftware(string SW1)
        {
            Logger.WriteLog(Path, "-------------------CHECK SW VERSION-----------------------");
            if (Putty.GetSoftwareVersion().Contains(SW1))
            {
                Logger.WriteLog(Path, "SW Verion true : " + SW1);
                return true;
            }
            Logger.WriteLog(Path, "SW Verion FAIL : " + SW1);
            return false;
        }

        public bool CheckPing(string IP)
        {
            if (!Ping(IP))
            {
                return false;
            }

            return true;
        }

        public bool SimplePing(string hostIP)
        {
            try
            {
                Ping pingSender = new Ping();
                System.Net.NetworkInformation.PingReply reply = pingSender.Send(hostIP);
                if (reply.Status == IPStatus.Success)
                {
                    Logger.WriteLog(Path,"Address: " + reply.Address.ToString());
                    Logger.WriteLog(Path, "Connection status: " + reply.Status);
                    return true;
                }
                else
                {
                    Logger.WriteLog(Path, "Connection status: " + reply.Status);
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog(Path, "Exception: " + e.Message);
                return false;
            }

        }
        private bool outTimeFlag = false;
        private System.Windows.Forms.Timer ForPause_timer;
        private void Delay_Time(int delaytime)
        {
            outTimeFlag = false;
            ForPause_timer.Interval = delaytime;
            ForPause_timer.Enabled = true;
            do
            {
                Application.DoEvents();

            }
            while (outTimeFlag == false);
            ForPause_timer.Enabled = false;

        }
        public void DelayMs(int ms)
        {
            int index = ms / 100 + 1;
            for (int i = 0; i < index; i++)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
        }
        public bool Ping(string hostip)
        {
            Logger.WriteLog(Path, "================Ping=========================");
            bool connection_flag = false;
            int looptimes = 0;
            do
            {
                connection_flag = SimplePing(hostip);
                if (connection_flag)
                {
                    Thread.Sleep(1000);
                    break;
                }

                looptimes++;
            }
            while (looptimes < 50);
            if (looptimes > 2)
            {
                DelayMs(10000);
            }
            return connection_flag;

        }
        public bool Ping2(string IP)
        {
            bool connect_flag = SimplePing(IP);
            for (int i = 0; i <= 100; i++)
            {

                if (connect_flag)
                {
                    Thread.Sleep(1000);
                    break;
                }

                if (i == 99)
                {
                    return false;
                }
            }
            return connect_flag;
        }
    }
}
