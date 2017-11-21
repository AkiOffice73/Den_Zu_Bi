using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;     // 多執行緒

namespace Den_Zu_Bi
{
    public partial class Form1 : Form
    {
        static Form1 Singleton;
        public Form1()
        {
            Singleton = this;
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            List<string> listPorts = new List<string>(ports);
            Comparison<string> comparer = delegate (string name1, string name2)
            {
                int port1 = Convert.ToInt32(name1.Remove(0, 3));
                int port2 = Convert.ToInt32(name2.Remove(0, 3));
                return (port1 - port2);
            };
            listPorts.Sort(comparer);
            comboBox1.Items.AddRange(listPorts.ToArray());
            ////////////////////////////////////////////////////////////////////////////////////

            Start.Enabled = true;
            Stop.Enabled = false;
            comboBox1.Enabled = true;
            refresh.Enabled = true;
            textBox1.Enabled = true;
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            List<string> listPorts = new List<string>(ports);
            Comparison<string> comparer = delegate (string name1, string name2)
            {
                int port1 = Convert.ToInt32(name1.Remove(0, 3));
                int port2 = Convert.ToInt32(name2.Remove(0, 3));
                return (port1 - port2);
            };
            listPorts.Sort(comparer);
            comboBox1.Items.AddRange(listPorts.ToArray());
        }


        SerialPort comPort;
        bool is_comPort_Open = false;
        private void Start_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                string comPort_Name = "";
                comPort_Name = comboBox1.Text;
                comPort = new SerialPort(comPort_Name, 115200, Parity.None, 8, StopBits.One);
                comPort.DataReceived += new SerialDataReceivedEventHandler(comport_DataReceived);
                Byte[] buffer = new Byte[512];
                if (!comPort.IsOpen)
                {
                    comPort.Open();
                }
                is_comPort_Open = true;

                Start.Enabled = false;
                Stop.Enabled = true;
                comboBox1.Enabled = false;
                refresh.Enabled = false;
                textBox1.Enabled = false;

                send_Command_Thread_Start();
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            comPort.Close();
            is_comPort_Open = false;
            Start.Enabled = true;
            Stop.Enabled = false;
            comboBox1.Enabled = true;
            refresh.Enabled = true;
            textBox1.Enabled = true;

            send_Command_Thread_Stop();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Thread send_Command_Thread;
        void send_Command_Thread_Start() {
            send_Command_Thread = new Thread(send_Command);
            send_Command_Thread.IsBackground = true;
            send_Command_Thread.Start();
        }
        public void send_Command_Thread_Stop()
        {
            if (send_Command_Thread != null && send_Command_Thread.IsAlive)
                send_Command_Thread.Abort();
        }
        void send_Command()
        {
            try
            {
                while (is_comPort_Open)
                {
                    comPort.Write("PRS\r");
                    Singleton.Invoke(new ResetTextBox(resetTextBox));
                    Thread.Sleep(Convert.ToInt32(textBox1.Text) * 1000);
                }
            }
            catch (Exception e) { }
        }
        delegate void ResetTextBox();
        private void resetTextBox()
        {
            recDataIndex = 0;
            rs_1.Text = "";
            rs_2.Text = "";
            rs_3.Text = "";
            rs_4.Text = "";
            rs_5.Text = "";
            rs_6.Text = "";
            rs_7.Text = "";
            rs_8.Text = "";
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //private void TEST_Click(object sender, EventArgs e)
        //{
        //    if (is_comPort_Open)
        //    {
        //        comPort.Write("PRS\r");
        //        recDataIndex = 0;
        //        rs_1.Text = "";
        //        rs_2.Text = "";
        //        rs_3.Text = "";
        //        rs_4.Text = "";
        //        rs_5.Text = "";
        //        rs_6.Text = "";
        //        rs_7.Text = "";
        //        rs_8.Text = "";
        //    }
        //}
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        delegate void Display(Byte[] buffer);
        int recDataIndex = 0;
        private void DisplayText(Byte[] buffer)
        {
            //TEST_Label.Text += System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            string get_Data = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            string[] rs_Data = get_Data.Split(new String[] { "\r\n" }, 9, StringSplitOptions.None);

            for (int i = 0; i < rs_Data.Length; i++)
            {
                if (rs_Data[i] == "PRS" || rs_Data[i] == ">")
                {
                    continue;
                }
                switch (recDataIndex)
                {
                    case 0:
                        rs_1.Text += rs_Data[i];
                        break;
                    case 1:
                        rs_2.Text += rs_Data[i];
                        break;
                    case 2:
                        rs_3.Text += rs_Data[i];
                        break;
                    case 3:
                        rs_4.Text += rs_Data[i];
                        break;
                    case 4:
                        rs_5.Text += rs_Data[i];
                        break;
                    case 5:
                        rs_6.Text += rs_Data[i];
                        break;
                    case 6:
                        rs_7.Text += rs_Data[i];
                        break;
                    case 7:
                        rs_8.Text += rs_Data[i];
                        break;
                    default:
                        new Exception("recDataIndex out of range = " + recDataIndex);
                        break;
                }
                if (i < rs_Data.Length - 1)
                {
                    recDataIndex++;
                }
            }

        }

        private void comport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new Byte[1024];
            Int32 length = (sender as SerialPort).Read(buffer, 0, buffer.Length);
            var a = (sender as SerialPort).ReadTimeout;
            Array.Resize(ref buffer, length);
            Display d = new Display(DisplayText);
            this.Invoke(d, new Object[] { buffer });
        }
    }
}
