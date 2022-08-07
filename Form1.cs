using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ZedGraph;
using System.Threading;

namespace DataLoaging_ver._1
{
    public partial class Form1 : Form
    {
        Thread main_thread;
        Thread sub_thread_1;

        LineItem li,li2;
        PointPairList ppl,ppl2;
        GraphPane gp;

        Stopwatch sw;

        setting settingform;

        public static string PortName = "NONE", SavePath= "NONE";

        string rawData=null, rawData2=null;

        int graph_status=0;
        

        public Form1()
        {
            string start_icon=(Environment.CurrentDirectory).Remove(Environment.CurrentDirectory.IndexOf("\\bin")-18) + "\\icons\\play.ico"
                , stop_icon= (Environment.CurrentDirectory).Remove(Environment.CurrentDirectory.IndexOf("\\bin") - 18) + "\\icons\\stop.ico";
            InitializeComponent();
            playToolStripMenuItem.Image = Image.FromFile(@start_icon);
            stopToolStripMenuItem.Image = Image.FromFile(@stop_icon);
            //label4.Text = DateTime.Now.ToString();
            label4.Text = "NONE";
            label6.Text = "NONE";
            label7.Text = "NONE";
            main_thread = new Thread(() => main_thread_func());
            main_thread.Start();

            graph_initializing();
        }
        void graph_initializing()
        {
            gp = zedGraphControl1.GraphPane;
            gp.Fill = new Fill(Color.White, Color.White, 180.0f);
            gp.XAxis.Title.Text = "Time";
            gp.YAxis.Title.Text = "Sound Level (dB)";
            gp.Title.Text = "Sound Level";
            gp.XAxis.MajorGrid.IsVisible = true;
            gp.XAxis.MinorGrid.IsVisible = true;
            gp.YAxis.MajorGrid.IsVisible = true;
            gp.YAxis.MinorGrid.IsVisible = true;

        }
        void main_thread_func()
        {
            while (true){label4.Text= DateTime.Now.ToString();
                label6.Text = PortName;
                label7.Text = SavePath;
            }
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                sub_thread_1 = new Thread(() => serial_run());
                sub_thread_1.Start();
            }
            catch
            {}
        }
        void serial_run()
        {
            double x=0, y=0, y2=0;

            if (graph_status != 0)
            {
                ppl.Clear();
                li.Clear();
                zedGraphControl1.GraphPane.CurveList.Clear();
                graph_status = 0;
            }

            graph_initializing();

            ppl = new PointPairList();
            ppl2 = new PointPairList();
            li=gp.AddCurve("LAF", ppl,Color.Red,SymbolType.None);
            li2 = gp.AddCurve("Leq", ppl2, Color.Blue, SymbolType.None);
            
            sw = new Stopwatch();
            sw.Start();

            if(!serialPort1.IsOpen)
            {
                serialPort1.PortName = PortName;
                serialPort1.BaudRate = 9600;
                serialPort1.Open();
            }

            while (true)
            {
                x= Convert.ToDouble(sw.ElapsedMilliseconds)/1000;
                serialPort1.WriteLine("DOD?");
                rawData = serialPort1.ReadExisting();
                if (rawData.IndexOf("R+0000") != -1)
                {
                    textBox1.Text="LAF : "+rawData.Substring(rawData.IndexOf("R+0000") + 9, 4)+" dB";
                    textBox2.Text = "Leq : " + rawData.Substring(rawData.IndexOf("R+0000") + 14, 5) + " dB";
                    y =Convert.ToDouble(rawData.Substring(rawData.IndexOf("R+0000") + 9, 4));
                    if(rawData.Substring(rawData.IndexOf("R+0000") + 14, 5).IndexOf("--")==-1)
                    y2= Convert.ToDouble(rawData.Substring(rawData.IndexOf("R+0000") + 14, 5));
                }
                ppl.Add(x, y);
                ppl2.Add(x, y2);
                zedGraphControl1.Refresh();
                zedGraphControl1.AxisChange();
                Thread.Sleep(100);
            }

            //if(PortName!="NONE")
            {
                



            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            
            sub_thread_1.Abort();
            graph_status = 1;
            /*z1.GraphPane.CurveList.Clear();
            z1.GraphPane.GraphObjList.Clear();
            z1.GraphPane.Title.Text = "";
            z1.GraphPane.XAxis.Title = "";
            z1.GraphPane.YAxis.Title = "";
            z1.GraphPane.Y2Axis.Title = "";
            z1.GraphPane.XAxis.Type = AxisType.Linear;
            z1.GraphPane.XAxis.Scale.TextLabels = null;
            z1.RestoreScale(z1.GraphPane);
            z1.AxisChange();
            z1.Invalidate();*/
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exiting();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                exiting();
            }
            catch
            {

            }
        }
        void exiting()
        {
            main_thread.Abort();
            sub_thread_1.Abort();
            Application.Exit();
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingform = new setting();
            settingform.ShowDialog();
        }
    }
}