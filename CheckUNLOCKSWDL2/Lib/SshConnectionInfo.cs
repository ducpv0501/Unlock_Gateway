using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Threading;
using Tamir.SharpSsh;
using System.Windows.Forms;


namespace CheckUNLOCKSWDL2.Lib
{
    public class SshConnectionInfo
    {
        public DataGridViewRow dr;
        public string Host { get; internal set; }
        public string IdentityFile { get; internal set; }
        public string Pass { get; internal set; }
        public string User { get; internal set; }
        public string port { get; internal set; }
    }
    internal class sshHelper
    {

        public bool Connected = false;
        private SshShell ss = null;
        private Stream io = null;
        private SshConnectionInfo scInfo;

        private byte[] buffer;
        private int bufSize = 256;
        private AsyncCallback readCallback;

        private delegate void addLineDelegate(string s);

        public sshHelper(SshConnectionInfo scInfo)
        {

            this.scInfo = scInfo;
            try
            {
                ss = new SshShell(scInfo.Host, scInfo.User);
                if (scInfo.Pass != null)
                {
                    ss.Password = scInfo.Pass;
                }
                if (scInfo.IdentityFile != null)
                {
                    ss.AddIdentityFile(scInfo.IdentityFile);
                }
                AddLine("Connect:" + scInfo.Host);
                ss.Connect(22);
                Connected = true;
                io = ss.GetStream();
                buffer = new byte[bufSize];
                readCallback = new AsyncCallback(OnCompletedRead);
                io.BeginRead(buffer, 0, bufSize, readCallback, null);
            }
            catch (Exception ex)
            {
                Connected = false;
                AddLine("Terminated yet!" + ex.Message);
            }
            AddLine("Connect:" + (Connected ? "Pass" : "Failed"));
            Thread.Sleep(500);
        }

        public void Close()
        {
            try
            {
                if (ss.Connected)
                {
                    io.Close();
                    ss.Close();
                }
            }
            catch { }
        }

        public string ReadData = "";
        //public string ReadTotal = "";
        private void OnCompletedRead(IAsyncResult ar)
        {
            int bytesRead = io.EndRead(ar);

            if (bytesRead > 0)
            {
                String str = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                AddLine(str);
                ReadData += str;
                //ReadTotal += str;
                //this.Invoke(new addLineDelegate(addLine), new object[] { str });
                io.BeginRead(buffer, 0, bufSize, readCallback, null);
            }
        }

        public void AddLine(string s)
        {
            scInfo.dr.Cells["col_Note"].Value = s;
            //Console.Write(scInfo.Host + ":" + s);
        }

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
        public bool SendCmd(string cmd)
        {
            ReadData = "";
            try
            {
                AddLine("SendCmd:" + cmd);
                Thread.Sleep(100);
                StreamWriter sw = new StreamWriter(io);
                sw.Write(cmd + '\n');
                sw.Flush();
                Thread.Sleep(100);
                dt = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Connected = false;
                AddLine("Terminated yet!" + ex.Message);
            }
            return false;
        }
    }
}
