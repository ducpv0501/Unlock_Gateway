using CheckUNLOCKSWDL2.Lib;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckUNLOCKSWDL2
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            LoadSetting(@"D:\Projector\CheckUNLOCKSWDL2\Setting.ini");
            SetTextBoxState(SLot);
        }
        Logger Logger = new Logger();
        #region config delacare
        public int SLot;
        const int SN_LENGTH = 12;
        public string MODEL_NAME;
        public string SW_1;
        public string SW_2;
        public string Port_1;
        public string Port_2;
        public string Port_3;
        public string Port_4;
        public string Port_5;
        public string Port_6;
        public string Port_7;
        public string Port_8;
       

        private Thread threadSN01;
        private Thread threadSN02;
        private Thread threadSN03;
        private Thread threadSN04;
        private Thread threadSN05;
        private Thread threadSN06;
        private Thread threadSN07;
        private Thread threadSN08;
        #endregion
        #region flag
        private bool stopThreadSN01 = false;
        private bool stopThreadSN02 = false;
        private bool stopThreadSN03 = false;
        private bool stopThreadSN04 = false;
        private bool stopThreadSN05 = false;
        private bool stopThreadSN06 = false;
        private bool stopThreadSN07 = false;
        private bool stopThreadSN08 = false;


        private void SetTextBoxState(int slot)
        {
            // Số lượng tối đa cho TextBox
            int maxTextBoxes = 8;

            for (int i = 1; i <= maxTextBoxes; i++)
            {
                // Xử lý nhóm txtSN
                var textBoxSN = this.Controls.Find($"txtSN{i:D2}", true).FirstOrDefault() as TextBox;

                if (textBoxSN != null)
                {
                    if (i <= slot)
                    {
                        textBoxSN.ReadOnly = false; 
                        textBoxSN.BackColor = Color.White; 
                    }
                    else
                    {
                        textBoxSN.ReadOnly = true; 
                        textBoxSN.BackColor = Color.LightGray; 
                    }
                }

                // Xử lý nhóm txtStatus
                var textBoxStatus = this.Controls.Find($"txtStatus{i:D2}", true).FirstOrDefault() as TextBox;

                if (textBoxStatus != null)
                {
                    if (i <= slot)
                    {
                        textBoxStatus.ReadOnly = false;
                        textBoxStatus.BackColor = Color.White;
                    }
                    else
                    {
                        textBoxStatus.ReadOnly = true;
                        textBoxStatus.BackColor = Color.LightGray; 
                    }
                }
            }
        }


        private void Process_Kill_new(string process)
        {
            try
            {
                Process[] proc = Process.GetProcesses();
                foreach (Process item in proc)
                {
                    if (item.ProcessName.Contains(process))
                    {
                        item.Kill();

                    }
                }
            }
            catch (Exception ep)
            {

            }
        }
        #endregion
        public string ScanSN(string text)
        {

            string key = @"""S"":""";
            int startIndex = text.IndexOf(key);
            if (startIndex >= 0)
            {

                startIndex += key.Length;
                int endIndex = text.IndexOf("\"", startIndex);
                if (endIndex > startIndex)
                {
                    return text.Substring(startIndex, endIndex - startIndex);
                }
            }
            return text;
        }
        public void CheckUnlockForSN(TextBox textBox, string DUT_SN, string IP, ref bool stopThread)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string Pathlog = $@"D:\A_De\Logs\{DUT_SN}_{timestamp}.txt";
            SeleniumHelper seleniumHelper = new SeleniumHelper(Pathlog);
            LibCommand lib = new LibCommand(Pathlog);
            try
            {
                RenderMessageToTextBox(textBox, Color.Yellow, Color.Black, "CHECK PING");
                if (stopThread) return;

                if (!lib.CheckPing(IP))
                {
                    RenderMessageToTextBox(textBox, Color.Red, Color.White, "PING FAIL");

                    Logger.WriteLog(Pathlog, "END CHECKING: PING FAIL");
                    return;
                }
                RenderMessageToTextBox(textBox, Color.Yellow, Color.Black, "CHECK SW");
                if (stopThread) return;

                if (lib.SSHConnect(IP))
                {
                    if (stopThread) return;
                    if (lib.CheckRunningSoftware(SW_1))
                    {

                        Logger.WriteLog(Pathlog, "END CHECKING: RETEST SWDL 2");
                        RenderMessageToTextBox(textBox, Color.Green, Color.White, "RETEST SWDL 2");
                        return;
                    }

                }

                RenderMessageToTextBox(textBox, Color.Yellow, Color.Black, "CHECK WEB SSH");
                if (stopThread) return;
                Thread.Sleep(5000);
                if (seleniumHelper.CheckWebStatus($"http://{IP}/web_whw/#/prelogin", DUT_SN)) //login default password
                {
                    if (stopThread) return;
                    if (lib.SSHConnect(IP))
                    {

                        Logger.WriteLog(Pathlog, "END CHECKING: RETEST SWDL 2");
                        RenderMessageToTextBox(textBox, Color.Green, Color.White, "RETEST SWDL 2");
                        return;
                    }
                    else
                    {

                        RenderMessageToTextBox(textBox, Color.Green, Color.White, "TE CHECK");
                        return;
                    }
                }
                if (stopThread) return;
                if (!seleniumHelper.CheckWebStatus($"http://{IP}/web_whw/#/prelogin", DUT_SN)) //login with super password
                {
                    if (stopThread) return;
                    if (!lib.SSHConnect(IP))
                    {

                        Logger.WriteLog(Pathlog, "END CHECKING: UNLOCK");
                        RenderMessageToTextBox(textBox, Color.Blue, Color.White, "UNLOCK");
                        return;
                    }
                    else
                    {

                        RenderMessageToTextBox(textBox, Color.Green, Color.White, "TE CHECK");
                        return;
                    }
                }



            }
            catch (Exception ex)
            {

                Logger.WriteLog(Pathlog, "Exception: " + ex.Message);
                RenderMessageToTextBox(textBox, Color.OrangeRed, Color.White, "UNKNOWN EXCEPTION");
            }
            finally
            {
                seleniumHelper.DestroyChrome();

            }

        }

        private void RenderMessageToTextBox(TextBox textBox, Color color, Color textColor, string message)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.BackColor = color;
                    textBox.ForeColor = textColor;
                    textBox.Text = message;
                }));
            }
            else
            {
                textBox.BackColor = color;
                textBox.ForeColor = textColor;
                textBox.Text = message;
            }
        }



        public void LoadSetting(string fileName)
        {

            StringBuilder Setting_temp = new StringBuilder(256);
            GetPrivateProfileString("CommonSetting", "ModelName", "", Setting_temp, Setting_temp.Capacity, fileName);
            MODEL_NAME = Setting_temp.ToString();
            txtModelName.Text = MODEL_NAME;
            GetPrivateProfileString("CommonSetting", "SW1", "", Setting_temp, Setting_temp.Capacity, fileName);
            SW_1 = Setting_temp.ToString();
            txtSW1.Text = SW_1;
            GetPrivateProfileString("CommonSetting", "SW2", "", Setting_temp, Setting_temp.Capacity, fileName);
            SW_2 = Setting_temp.ToString();
            txtSW2.Text = SW_2;
            GetPrivateProfileString("CommonSetting", "Slot", "", Setting_temp, Setting_temp.Capacity, fileName);
            SLot = Convert.ToInt32(Setting_temp.ToString());
            GetPrivateProfileString("PortSetting", "Port_1", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_1 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_2", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_2 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_3", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_3 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_4", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_4 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_5", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_5 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_6", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_6 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_7", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_7 = Setting_temp.ToString();
            GetPrivateProfileString("PortSetting", "Port_8", "", Setting_temp, Setting_temp.Capacity, fileName);
            Port_8 = Setting_temp.ToString();
            //GetPrivateProfileString("CommonSetting", "ImageVersion", "", Setting_temp, Setting_temp.Capacity, fileName);

        }
        [DllImport("kernel32")]
        public static extern bool GetPrivateProfileString(string lpApplicationName,
          string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        #region TextBox 1 Event
        //private void txtSN01_KeyPress(object sender, KeyPressEventArgs e)
        //{

        //    if (e.KeyChar == (char)125)
        //    { 
        //        string DUT_SN = ScanSN(txtSN01.Text.Trim());
        //        txtSN01.Text = DUT_SN;
        //        if (txtSN01 == null) return;
        //        if (string.IsNullOrEmpty(DUT_SN))
        //        {
        //            MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        txtSN01.ReadOnly = true;
        //        txtStatus01.ReadOnly = true;
        //        if (threadSN01 == null || !threadSN01.IsAlive)
        //        {
        //            stopThreadSN01 = false;

        //            threadSN01 = new Thread(() =>
        //            {

        //                CheckUnlockForSN(txtStatus01, DUT_SN, Port_1, ref stopThreadSN01);
        //                txtSN01.Text = "";

        //            });                  

        //            threadSN01.IsBackground = true;
        //            threadSN01.Start();

        //        }

        //        //e.Handled = true;
        //    }
        //}
        private void txtSN01_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN01.Text.Trim());
              

                if (string.IsNullOrEmpty(DUT_SN)&&DUT_SN.Length!=SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN01.Text = DUT_SN;
                txtSN01.ReadOnly = true;
                txtStatus01.ReadOnly = true;

                if (threadSN01 == null || !threadSN01.IsAlive)
                {
                    stopThreadSN01 = false;

                    threadSN01 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus01, DUT_SN, Port_1, ref stopThreadSN01);
                        }
                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN01.Clear();
                                txtStatus01.ReadOnly = false;
                                txtSN01.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN01.IsBackground = true;
                    threadSN01.Start();
                }
            }
        }
        private void btnSN01_Click(object sender, EventArgs e)
        {
            if (threadSN01 != null && threadSN01.IsAlive)
            {
                RenderMessageToTextBox(txtStatus01, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN01 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN01.Join(); // Chờ luồng kết thúc
                    threadSN01 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN01.Clear();
                        txtStatus01.ReadOnly = false;
                        txtSN01.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus01, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }
        }

        #endregion
        #region TextBox 2 Event
        private void txtSN02_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN02.Text.Trim());
                txtSN02.Text = DUT_SN;

                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN02.ReadOnly = true;
                txtStatus02.ReadOnly = true;

                if (threadSN02 == null || !threadSN02.IsAlive)
                {
                    stopThreadSN02 = false;

                    threadSN02 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus02, DUT_SN, Port_2, ref stopThreadSN02);
                        }
                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN02.Clear();
                                txtStatus02.ReadOnly = false;
                                txtSN02.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN02.IsBackground = true;
                    threadSN02.Start();
                }
            }
        }

        private void btnreset02_Click(object sender, EventArgs e)
        {
            if (threadSN02 != null && threadSN02.IsAlive)
            {
                RenderMessageToTextBox(txtStatus02, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN02 = true; 

              
                Task.Run(() =>
                {
                    threadSN02.Join(); 
                    threadSN02 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN02.Clear();
                        txtStatus02.ReadOnly = false;
                        txtSN02.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus02, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }
        }

        #endregion
        #region TextBox 3 Event
        private void txtSN03_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)125) 
            {
                string DUT_SN = ScanSN(txtSN03.Text.Trim());
               

                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN03.Text = DUT_SN;
                txtSN03.ReadOnly = true;
                txtStatus03.ReadOnly = true;

                if (threadSN03 == null || !threadSN03.IsAlive)
                {
                    stopThreadSN03 = false;

                    threadSN03 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus03, DUT_SN, Port_3, ref stopThreadSN03);
                        }

                        finally
                        {
                           
                            Invoke(new Action(() =>
                            {
                                txtSN03.Clear();
                                txtStatus03.ReadOnly = false;
                                txtSN03.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN03.IsBackground = true;
                    threadSN03.Start();
                }
            }
        }



        private void btnStop03_Click(object sender, EventArgs e)
        {

            if (threadSN03 != null && threadSN03.IsAlive)
            {
                RenderMessageToTextBox(txtStatus03, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN03 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN03.Join(); // Chờ luồng kết thúc
                    threadSN03 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN03.Clear();
                        txtStatus03.ReadOnly = false;
                        txtSN03.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus03, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }
        }
        #endregion
        #region TextBox 4 Event
        private void txtSN04_DoubleClick(object sender, EventArgs e)
        {

            if (threadSN04 == null)
            {
                txtSN04.Clear();
                txtSN04.ReadOnly = false;
                txtStatus04.Clear();
                txtStatus04.BackColor = SystemColors.Window;
            }
        }

        private void txtSN04_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN04.Text.Trim());
               

                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN04.Text = DUT_SN;

                txtSN04.ReadOnly = true;
                txtStatus04.ReadOnly = true;

                if (threadSN04 == null || !threadSN04.IsAlive)
                {
                    stopThreadSN04 = false;

                    threadSN04 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus04, DUT_SN, Port_4, ref stopThreadSN04);
                        }

                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN04.Clear();
                                txtStatus04.ReadOnly = false;
                                txtSN04.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN04.IsBackground = true;
                    threadSN04.Start();
                }
            }
        }

        private void btnStop04_Click(object sender, EventArgs e)
        {
            if (threadSN04 != null && threadSN04.IsAlive)
            {
                RenderMessageToTextBox(txtStatus04, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN04 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN04.Join(); // Chờ luồng kết thúc
                    threadSN04 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN04.Clear();
                        txtStatus04.ReadOnly = false;
                        txtSN04.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus04, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }


        }
        #endregion
        #region Textbox 5 Event
        private void txtSN05_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN05.Text.Trim());
                

                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN05.Text = DUT_SN;
                txtSN05.ReadOnly = true;
                txtStatus05.ReadOnly = true;

                if (threadSN05 == null || !threadSN05.IsAlive)
                {
                    stopThreadSN05 = false;

                    threadSN05 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus05, DUT_SN, Port_5, ref stopThreadSN05);
                        }

                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN05.Clear();
                                txtStatus05.ReadOnly = false;
                                txtSN05.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN05.IsBackground = true;
                    threadSN05.Start();
                }
            }
        }

   

        private void btnStopSN05_Click(object sender, EventArgs e)
        {

            if (threadSN05 != null && threadSN05.IsAlive)
            {
                RenderMessageToTextBox(txtStatus05, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN05 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN05.Join(); // Chờ luồng kết thúc
                    threadSN05 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN05.Clear();
                        txtStatus05.ReadOnly = false;
                        txtSN05.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus05, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }



        }
        #endregion
        #region Textbox 6 Event  
        private void txtSN06_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN06.Text.Trim());
              
                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN06.Text = DUT_SN;
                txtSN06.ReadOnly = true;
                txtStatus06.ReadOnly = true;

                if (threadSN06 == null || !threadSN06.IsAlive)
                {
                    stopThreadSN06 = false;

                    threadSN06 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus06, DUT_SN, Port_6, ref stopThreadSN06);
                        }

                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN06.Clear();
                                txtStatus06.ReadOnly = false;
                                txtSN06.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN06.IsBackground = true;
                    threadSN06.Start();
                }
            }
        }

        private void btnStopSN06_Click(object sender, EventArgs e)
        {
            if (threadSN06 != null && threadSN06.IsAlive)
            {
                RenderMessageToTextBox(txtStatus06, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN06 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN06.Join(); // Chờ luồng kết thúc
                    threadSN06 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN06.Clear();
                        txtStatus06.ReadOnly = false;
                        txtSN06.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus06, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }

        }
        #endregion
        #region Textbox 7 Event
        private void txtSN07_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN07.Text.Trim());
              
                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN07.Text = DUT_SN;
                txtSN07.ReadOnly = true;
                txtStatus07.ReadOnly = true;

                if (threadSN07 == null || !threadSN07.IsAlive)
                {
                    stopThreadSN07 = false;

                    threadSN07 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus07, DUT_SN, Port_7, ref stopThreadSN07);
                        }

                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN07.Clear();
                                txtStatus07.ReadOnly = false;
                                txtSN07.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN07.IsBackground = true;
                    threadSN07.Start();
                }
            }

        }

 
        private void btnStopSN07_Click(object sender, EventArgs e)
        {
            if (threadSN07 != null && threadSN07.IsAlive)
            {
                RenderMessageToTextBox(txtStatus07, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN07 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN07.Join(); // Chờ luồng kết thúc
                    threadSN07 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN07.Clear();
                        txtStatus07.ReadOnly = false;
                        txtSN07.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus07, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }
        }
        #endregion
        #region Textbox 8 Event

        private void txtSN08_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == (char)125) // Nếu nhấn phím "}"
            {
                string DUT_SN = ScanSN(txtSN08.Text.Trim());
               

                if (string.IsNullOrEmpty(DUT_SN) && DUT_SN.Length != SN_LENGTH)
                {
                    MessageBox.Show("Serial Number không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtSN08.Text = DUT_SN;
                txtSN08.ReadOnly = true;
                txtStatus08.ReadOnly = true;

                if (threadSN08 == null || !threadSN08.IsAlive)
                {
                    stopThreadSN08 = false;

                    threadSN08 = new Thread(() =>
                    {
                        try
                        {
                            CheckUnlockForSN(txtStatus08, DUT_SN, Port_8, ref stopThreadSN08);
                        }

                        finally
                        {
                            // Đặt lại trạng thái sau khi luồng kết thúc
                            Invoke(new Action(() =>
                            {
                                txtSN08.Clear();
                                txtStatus08.ReadOnly = false;
                                txtSN08.ReadOnly = false;
                            }));
                        }
                    });

                    threadSN08.IsBackground = true;
                    threadSN08.Start();
                }
            }

        }

        private void btnStopSN08_Click(object sender, EventArgs e)
        {
            if (threadSN08 != null && threadSN08.IsAlive)
            {
                RenderMessageToTextBox(txtStatus08, Color.Orange, Color.White, "STOPPING...");
                stopThreadSN08 = true; // Cờ dừng

                // Chờ luồng tự dừng nếu cần
                Task.Run(() =>
                {
                    threadSN08.Join(); // Chờ luồng kết thúc
                    threadSN08 = null;

                    Invoke(new Action(() =>
                    {
                        txtSN08.Clear();
                        txtStatus08.ReadOnly = false;
                        txtSN08.ReadOnly = false;
                        RenderMessageToTextBox(txtStatus08, Color.Orange, Color.White, "STOPPED");
                    }));
                });
            }


        }
        #endregion
    }
}
