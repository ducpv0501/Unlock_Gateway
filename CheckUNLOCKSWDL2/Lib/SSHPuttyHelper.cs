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

    public class SSHPuttyHelper
    {
        public System.Windows.Forms.Timer ForPause_timer;
        SshConnectionInfo sshinfo;
        sshputty PUTTYSSH;
        Logger Logger = new Logger();
        public string Output_str = "";
        public bool outTimeFlag = false;
        public string plinkPath = @"C:\Program Files (x86)\PuTTY\plink.exe";
        public string Path;
        public SSHPuttyHelper()
        {

        }
        public SSHPuttyHelper(string _path)
        {
            this.Path = _path;  
        }
        public bool G5_SSH_connect_dut_putty(string Host, string User, string Pass, string port)
        {
            if (!File.Exists(plinkPath))
            {
                plinkPath = @"C:\Program Files\PuTTY\plink.exe";
            }
            sshinfo = new SshConnectionInfo();
            PUTTYSSH = new sshputty();
            sshinfo.Host = Host;
            sshinfo.User = User;
            sshinfo.Pass = Pass;
            sshinfo.port = port;
            Output_str = "";
            bool return_f = PUTTYSSH.PlinkSSh(sshinfo);
            Output_str = PUTTYSSH.ReadData; 
            Logger.WriteLog(Path,"SSH Output: " + Output_str);
            return return_f;

        }

        private bool G5_SSH_SendCMD_WaitFor_putty(string sendcmd, string expectstr, string passcriterion, int timeout)
        {
            Output_str = "";
            //PUTTYSSH.SendCmdAndWait("d 1", "#", 20);
            Logger.WriteLog(Path, "Send command with PUTTY: " + sendcmd);
            PUTTYSSH.SendCmdAndWait(sendcmd, expectstr, timeout);
            for (int i = 0; i < timeout; i++)
            {

                Output_str = PUTTYSSH.ReadData;
                try
                {
                    int ak_indexof = Output_str.IndexOf(sendcmd);
                    if (ak_indexof < 0)
                    {
                        ak_indexof = 0;
                    }
                    string ak = Output_str.Substring(ak_indexof, Output_str.Length - ak_indexof);
                    Output_str = ak;
                    if (ak.Contains(expectstr))
                    {
                        break;
                    }
                }
                catch (Exception ep)
                {
                    Logger.WriteLog(Path,"Exception : " +ep.Message);
                   Console.WriteLine(ep.ToString());
                    return false;
                }
                Thread.Sleep(2000);
            } 
            Logger.WriteLog(Path,"SSH Output: "+ PUTTYSSH.ReadData);
            try
            {
                if (Output_str.Contains(passcriterion))
                {
                    ////Main_Show_R(FCT_Logfile, "SSH Output:" + Output_str);
                    //Logger.WriteLog(Path,"Pass Criterion is : " +passcriterion);
                    return true;
                }
            }
            catch (Exception ep)
            {
                Logger.WriteLog(Path, "Exception : " + ep.Message);
                return false;
            }
            //Main_Show_R(FCT_Logfile, "SSH Output:" + Output_str);
             Logger.WriteLog(Path, "Pass Criterion is :(" + passcriterion + ")");
            return false;

        }
        public string GetSoftwareVersion()
        {
            Logger.WriteLog(Path, "========================Get SW Vertion With PUTTY ==============================");
            bool commandSent = G5_SSH_SendCMD_WaitFor_putty("d 1", "root@OpenW", "Running SW (File)", 30);

            if (commandSent)
            {
                // Tìm kiếm chuỗi "Running SW (File)" trong kết quả trả về
                int startIndex = Output_str.IndexOf("Running SW (File)");
                if (startIndex >= 0)
                {
                    // Tính chỉ số bắt đầu cho đoạn văn bản sau dấu ':'
                    startIndex = Output_str.IndexOf(":", startIndex) + 1;

                    if (startIndex > 0)
                    {
                        // Lấy chuỗi từ vị trí dấu ':' đến cuối dòng
                        int endIndex = Output_str.IndexOf("\n", startIndex);
                        string softwareVersion = Output_str.Substring(startIndex, endIndex - startIndex).Trim();

                        // Trả về phiên bản phần mềm đã tìm thấy
                        Logger.WriteLog(Path, "Software version is: "+ softwareVersion);
                        return softwareVersion;
                    }
                }
            }
            Logger.WriteLog(Path, "Not found SoftWare Version running!");         
            return "Không tìm thấy phiên bản phần mềm.";
        }
    }
}
