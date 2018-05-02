using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testing_Arduino_Gyro
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void ReloadComboBox()
        {
            comboBox1.Items.Clear();

            foreach (String port in SerialPort.GetPortNames()) comboBox1.Items.Add(port);
        }

        public void SetStatusText(String text)
        {
            label1.Text = text;
        }

        SerialPort serialPort;

        int[] stayOffset = { 0, 0, 0 };

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void UpdateAccOffsets()
        {
            label2.Text = "x: " + stayOffset[0] + "\n" +
                "y: " + stayOffset[1] + "\n" +
                "z: " + stayOffset[2];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReloadComboBox();
            SetStatusText("");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReloadComboBox();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string portName = comboBox1.Text;

            if(portName == "" || portName == null)
            {
                MessageBox.Show(this, "You have to select a port first", "Please, select a port", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            serialPort = new SerialPort(portName, 115200);
            SetStatusText("Connecting to " + portName + "...");
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.Open();
            SetStatusText("Connected!");
        }

        int[] lastReadValues = { 0, 0, 0 };

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int dataLength = serialPort.BytesToRead;
            byte[] data = new byte[dataLength];
            int nbrDataRead = serialPort.Read(data, 0, dataLength);
            if (nbrDataRead == 0)
                return;
            String read = Encoding.Default.GetString(data);
            //Console.WriteLine("Read: \"" + read + "\"");
            if (read.StartsWith("BEGIN"))
            {
                serialPort.Write(" ");
                SetStatusText("Beginned");
            }
            else if (read.StartsWith("aworld"))
            {
                String modRead = read.Replace("aworld\t", "").Replace("\n", "");
                String[] readParams = modRead.Split('\t');

                lastReadValues[0] = int.Parse(readParams[0]);
                lastReadValues[1] = int.Parse(readParams[1]);
                lastReadValues[2] = int.Parse(readParams[2]);

                SetAcc((lastReadValues[0] - stayOffset[0]).ToString(),
                    (lastReadValues[1] - stayOffset[1]).ToString(),
                    (lastReadValues[2] - stayOffset[2]).ToString());
            }
        }

        delegate void SetAccCallback(string valX, string valY, string valZ);

        private void SetAcc(string valX, string valY, string valZ)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (label6.InvokeRequired)
            {
                Invoke(new SetAccCallback(SetAcc), new object[] { valX, valY, valZ });
            }
            else
            {
                label6.Text = valX;
                label7.Text = valY;
                label8.Text = valZ;

                String newText = "";
                if (int.Parse(valX) > 20)
                {
                    newText += " X+";
                }else if(int.Parse(valX) < -20)
                {
                    newText += " X-";
                }
                if (int.Parse(valY) > 20)
                {
                    newText += " Y+";
                }
                else if (int.Parse(valY) < -20)
                {
                    newText += " Y-";
                }
                if (int.Parse(valZ) > 20)
                {
                    newText += " Z+";
                }
                else if (int.Parse(valZ) < -20)
                {
                    newText += " Z-";
                }
                label9.Text = newText;
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                stayOffset[0] = lastReadValues[0];
                stayOffset[1] = lastReadValues[1];
                stayOffset[2] = lastReadValues[2];

                UpdateAccOffsets();
            }
        }
    }
}
