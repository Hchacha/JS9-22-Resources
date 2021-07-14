using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Collections;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public SectionCapCal sectionCapCal;
        public string DataBasePath;//数据库地址
        public Form1()
        {
            InitializeComponent();
            sectionCapCal = new SectionCapCal();



        }


        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件


            dialog.InitialDirectory = Environment.CurrentDirectory.ToString();


        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;




        }

        private void ComboxRoutesName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // EcxelToListBox2(Application.StartupPath + "/Resources/四纵四横.xlsx", ComboxRoutesName.SelectedItem.ToString());
            //string path = Application.StartupPath;
            //string path1 = AppDomain.CurrentDomain.BaseDirectory;
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            
            EcxelToListBox2(filePath, ComboxRoutesName.SelectedItem.ToString(), listBoxStationData);
        }


        public void EcxelToListBox2(string filePath, string RouteName, ListBox listBox)
        {
            //根据路径打开一个Excel文件并将数据填充到DataSet中
            string strConn = "provider=Microsoft.ACE.OLEDB.12.0; Data Source='" + filePath + "'" + "; Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";//HDR=YES 有两个值:YES/NO,表示第一行是否字段名,默认是YES,第一行是字段名

            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            DataTable da = null;
            strExcel = "select * from   [sheet1$]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            da = ds.Tables[0];
            List<string> listName = da.AsEnumerable().Select(d => d.Field<string>(RouteName)).ToList();
            listBox.DataSource = listName;
        }

        private void listBoxStationData_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            string OpenTrackImgPath = path + "\\Resources\\车站图片\\OpenTrack\\" + listBoxStationData.SelectedItem.ToString() + ".png";
            if (File.Exists(OpenTrackImgPath))
                OpenTrackPictureBox.Image = Image.FromFile(OpenTrackImgPath);
            else
                MessageBox.Show("无法找到该站OpenTrack资源");
            OpenTrackPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            string Ts2ImgPath = path + "\\Resources\\车站图片\\Ts2\\" + listBoxStationData.SelectedItem.ToString().Substring(0, listBoxStationData.SelectedItem.ToString().Length - 1) + ".png";
            if (File.Exists(Ts2ImgPath))
                Ts2PictureBox.Image = Image.FromFile(Ts2ImgPath);
            else
                MessageBox.Show("无法找到该站Ts2资源");
            Ts2PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            string VisioImgPath = path + "\\Resources\\车站图片\\Visio\\" + listBoxStationData.SelectedItem.ToString().Substring(0, listBoxStationData.SelectedItem.ToString().Length - 1) + ".png";
            if (File.Exists(VisioImgPath))
                VisioPictureBox.Image = Image.FromFile(VisioImgPath);
            else
                MessageBox.Show("无法找到该站平面示意图资源");
            VisioPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;


            string mapPath = path+"\\Resources\\leaflet\\" + listBoxStationData.SelectedItem.ToString() + ".html";
            //webBrowser1.ScriptErrorsSuppressed = true;
            StaLocaWebBrowser.Navigate(mapPath);
            //webKitBrowser1.Url = new Uri(mapPath);
            //webKitBrowser1.Navigate(mapPath);
        }

        private void OpenTrackPictureBox_Click(object sender, EventArgs e)
        {


            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string cmdExe = path + "\\OpenTrackFile\\OpenTrackApp\\OpenTrack.exe";
            string cmdStr = "";
            
            string[] documentName = { "STA_BeiJingNan", "STA_LangFang", "????", "STA_TianJingNan", "STA_CangZhouXi", "STA_DeZhouDong", "STA_JiNanXi", "STA_TaiAn", "STA_QuFuDong", "STA_TengZhouDong", "STA_ZaoZhuang", "STA_XuZhouDong", "STA_SuZhouDong", "STA_BengBuNan", "STA_DingYuan", "STA_ChuZhou", "STA_NanJingNan", "STA_ZhenJiangNan", "STA_DanYangBei", "STA_ChangZhouBei", "STA_WuXiDong", "STA_SuZhouBei", "STA_KunShanNan", "STA_ShanHaiHongQiao" };
            if (listBoxStationData.SelectedIndex != 2)
            {

                //MessageBox.Show(path);
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(str + "&exit");

                p.StandardInput.AutoFlush = true;




                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                if (output != null)
                {
                    // MessageBox.Show("仿真结束！");
                    //MessageBox.Show(output);
                }
                //StreamReader reader = p.StandardOutput;
                //string line=reader.ReadLine();
                //while (!reader.EndOfStream)
                //{
                //    str += line + "  ";
                //    line = reader.ReadLine();
                //}

                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
                //Console.WriteLine(output);
            }
        }

        private void Station_CenterPanel_SizeChanged(object sender, EventArgs e)
        {
            Station_RightTopPanel.Height = (Station_RightPanel.Height / 2);
            Station_RightPanel.Width = (toolStrip2.Width - Station_LeftPanel.Width) / 2 - 50;
            Station_CenterTopPanel.Height = (Station_CenterPanel.Height / 2);
            Station_LeftBottomPanel.Height = Station_LeftPanel.Height - Station_LeftTopPanel.Height;

        }

        private void tabControlSection_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            if (tabControlSection.SelectedTab.Name == "setBlockParameter")
            {
                //string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string filePath = path + "\\Resources\\附表5 巴准线连续里程坡度表（内含4表）.xlsx";

                string strConn = "provider=Microsoft.ACE.OLEDB.12.0; Data Source='" + filePath + "'" + "; Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";//HDR=YES 有两个值:YES/NO,表示第一行是否字段名,默认是YES,第一行是字段名

                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                string strExcel = "";
                OleDbDataAdapter myCommand = null;
                DataSet ds = null;
                DataTable da = null;
                strExcel = "select  * from   [坡度表3$]";
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                ds = new DataSet();
                myCommand.Fill(ds, "table1");
                da = ds.Tables[0];
                blockDataGridView1.DataSource = da;

                panel1.Height = Block_CenterPanel.Height - groupBox1.Height;
            }
            if (tabControlSection.SelectedTab.Name == "setTrainsParameter")
            {
                //调整panel大小
                Trains_CenterBottomPanel.Height = (Trains_CenterPanel.Height / 2);
                Trains_CenterTopPanel.Height = (Trains_CenterPanel.Height / 2);

                //添加chart数据
                List<int> speed = new List<int>();
                List<double> effort = new List<double>();
                List<double> deceleration = new List<double>();
                for (int i = 0; i <= 35; i++)
                {
                    speed.Add(i * 10);
                    if (i < 12)
                        effort.Add(300 - 2.85 * i);
                    else
                        effort.Add(3150 / i);

                    if (i <= 8) deceleration.Add(0.68);
                    if (i > 8 && i <= 16) deceleration.Add(0.35);
                    if (i > 16 && i <= 25) deceleration.Add(0.20);
                    if (i > 25 && i <= 30) deceleration.Add(0.10);
                    if (i > 30 && i <= 35) deceleration.Add(0.05);
                }

                chart1.Series["CR400AF"].Points.DataBindXY(speed, effort);
                chart2.Series["CR400AF"].Points.DataBindXY(speed, deceleration);

                chart1.ChartAreas[0].AxisX.Minimum = 0;
                chart1.ChartAreas[0].AxisX.Maximum = 400;
                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart2.ChartAreas[0].AxisX.Maximum = 400;

            }
            if (tabControlSection.SelectedTab.Name == "OpenTrackSimulation")
            {
                OTSim_CenterBottomPanel.Height = (OTSim_CenterPanel.Height / 2);
                OTSim_CenterTopPanel.Height = (OTSim_CenterPanel.Height / 2);
                
                string filePath = path + @"\Resources\ChineseHighSpeedRailNetwork_all.html";
                webBrowser1.Navigate(filePath);

            }
            if (tabControlSection.SelectedTab.Name == "TS2Simulation")
            {
                //调整panel大小
                TS2Sim_CenterBottomPanel.Height = (TS2Sim_CenterPanel.Height / 2);
                TS2Sim_CenterTopPanel.Height = (TS2Sim_CenterPanel.Height / 2);

                string filePath = path + @"\Resources\ChineseHighSpeedRailNetwork_all.html";
                webBrowser5.Navigate(filePath);
            }
            if (tabControlSection.SelectedTab.Name == "Visualization")
            {
                //调整panel大小
                Visual_RightTopPanel.Height = (Visual_RightPanel.Height / 2);
                Visual_RightBottomPanel.Height = (Visual_RightPanel.Height / 2);
                Visual_LeftTopPanel.Height = (Visual_LeftPanel.Height / 2);
                Visual_LeftBottomPanel.Height = (Visual_LeftPanel.Height / 2);


                webBrowser2.Navigate(path + @"\Resources\Opentrack仿真延误分析-demo3(1).html");
                webBrowser3.Url = new Uri(path + @"\Resources\radar2.html");

                webBrowser4.Navigate(path + @"\Resources\速度距离曲线demo.html");

                List<int> speed = new List<int>();
                List<double> effort = new List<double>();
                for (int i = 0; i <= 35; i++)
                {
                    speed.Add(i * 10);
                    if (i < 12)
                        effort.Add(300 - 2.85 * i);
                    else
                        effort.Add(3150 / i);
                }

                chart3.Series["Series1"].Points.DataBindXY(speed, effort);
               
            }

        }

        private void tabControlSection_Selected(object sender, TabControlEventArgs e)
        {
            
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string filePath = path + "\\Resources\\线路区段列表.xlsx";
            EcxelToListBox2(filePath, comboBox5.SelectedItem.ToString(), listBox1);
        }

        private void toolStripButton32_Click(object sender, EventArgs e)
        {
            if (OTSim_StartTime3.Text == "")
                MessageBox.Show("请读取仿真参数！");
            else
            {
                string str = "start \"\" /max \"D:\\Program Files (x86)\\Opentrack\\OpenTrack.app\\OpenTrack.exe\" \"C:\\Users\\Administrator\\Desktop\\hzc\\STA_ChangZhouBei.opentrack\"";
                string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string cmdExe = path + "\\OpenTrackFile\\OpenTrackApp\\OpenTrack.exe";
                string filePath = path + "\\OpenTrackFile\\JingHu_HighSpeedRailway\\RailwayModel\\";
                string[] documentName = { "STA_BeiJingNan", "STA_LangFang", "STA_TianJingNan", "STA_CangZhouXi", "STA_DeZhouDong", "STA_JiNanXi", "STA_TaiAn", "STA_QuFuDong", "STA_TengZhouDong", "STA_ZaoZhuang", "STA_XuZhouDong", "STA_SuZhouDong", "STA_BengBuNan", "STA_DingYuan", "STA_ChuZhou", "STA_NanJingNan", "STA_ZhenJiangNan", "STA_DanYangBei", "STA_ChangZhouBei", "STA_WuXiDong", "STA_SuZhouBei", "STA_KunShanNan", "STA_ShanHaiHongQiao" };
                string runfilePath = path + "\\Resources\\demo1.txt";
                // cmdStr = filePath + documentName[4] + ".opentrack";
                //str = "start \"\" /max \"" + cmdExe + "\" \"" + cmdStr + "\"";
                str = "\"" + cmdExe + "\" -script -runfile=\"" + runfilePath + "\"";
                //MessageBox.Show(path);
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                DateTime beforDT = System.DateTime.Now;

                //耗时巨大的代码

                DateTime afterDT = System.DateTime.Now;


                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(str + "&exit");

                p.StandardInput.AutoFlush = true;
                //p.StandardInput.WriteLine("exit");

                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();


                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
                if (MessageBox.Show("仿真已结束，查看仿真结果", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    AnalysisForm form = new AnalysisForm();
                    form.ShowDialog();
                }

                pictureBox3.Image = WindowsFormsApp1.Properties.Resources.BBN2020_05_04;
                pictureBox4.Image = WindowsFormsApp1.Properties.Resources.仿真运行图demo;
            }


        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Name == "动车组")
            {
                OleDbConnection thisConnection = new OleDbConnection(@"provider=Microsoft.ACE.OLEDB.12.0;Data Source=行车记录.mdb");
                string sql = "select * from [动车组]";
                OleDbDataAdapter thisAdapter = new OleDbDataAdapter(sql, thisConnection);
                System.Data.DataSet thisDataSet = new System.Data.DataSet();
                thisAdapter.Fill(thisDataSet);
                DataTable dt = thisDataSet.Tables[0];
                this.dataGridView1.DataSource = dt;
                thisConnection.Close();
            }
            if (tabControl1.SelectedTab.Name == "动车组阻力系数")
            {
                OleDbConnection thisConnection = new OleDbConnection(@"provider=Microsoft.ACE.OLEDB.12.0;Data Source=行车记录.mdb");
                string sql = "select * from [动车组阻力系数]";
                OleDbDataAdapter thisAdapter = new OleDbDataAdapter(sql, thisConnection);
                System.Data.DataSet thisDataSet = new System.Data.DataSet();
                thisAdapter.Fill(thisDataSet);
                DataTable dt = thisDataSet.Tables[0];
                this.dataGridView2.DataSource = dt;
                thisConnection.Close();
            }
            if (tabControl1.SelectedTab.Name == "动车组定员")
            {
                OleDbConnection thisConnection = new OleDbConnection(@"provider=Microsoft.ACE.OLEDB.12.0;Data Source=行车记录.mdb");
                string sql = "select * from [动车组定员]";
                OleDbDataAdapter thisAdapter = new OleDbDataAdapter(sql, thisConnection);
                System.Data.DataSet thisDataSet = new System.Data.DataSet();
                thisAdapter.Fill(thisDataSet);
                DataTable dt = thisDataSet.Tables[0];
                this.dataGridView3.DataSource = dt;
                thisConnection.Close();
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            string strInto = @"cd D:\PythonCode\pyETRC\train_graph-master";
            string strRun = "python main.py";
            //MessageBox.Show(path);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine(strInto);

            p.StandardInput.WriteLine("d:");

            p.StandardInput.WriteLine(strRun + "&exit");
            p.StandardInput.AutoFlush = true;
            
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            p.Close();
        }

        private void toolStripButton63_Click(object sender, EventArgs e)
        {
            VisualPanel.Controls.Clear();
            AnalysisForm analysisForm = new AnalysisForm();
            analysisForm.TopLevel = false;
            analysisForm.Dock = DockStyle.Fill;
            analysisForm.FormBorderStyle = FormBorderStyle.None;
            VisualPanel.Controls.Add(analysisForm);
            analysisForm.Show();

        }

        private void toolStripButton43_Click(object sender, EventArgs e)
        {
            if (TS2Sim_StartTime3.Text == "")
            {
                MessageBox.Show("请读取仿真参数！");
            }
            else
            {
                pictureBox6.Image = WindowsFormsApp1.Properties.Resources.镇江南;
                pictureBox7.Image = WindowsFormsApp1.Properties.Resources.上海虹桥;
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            StationInformation stationDataForm = new StationInformation();
            stationDataForm.ShowDialog();
        }

        private void Ts2PictureBox_Click(object sender, EventArgs e)
        {
           

           
            string str1 = "d:";
            string str2 = "cd D:\\PythonCode\\ts2-master";
            string str3 = "python start-ts2.py";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

                //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str1);
            p.StandardInput.WriteLine(str2);
            p.StandardInput.WriteLine(str3 + "&exit");

            p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();
           // MessageBox.Show(output);
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            //}
        }

        private void toolStripButton79_Click(object sender, EventArgs e)
        {
            ComparisonForm form = new ComparisonForm();
            form.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OTSim_StartTime0.Text = "0";
            OTSim_StartTime1.Text = "08";
            OTSim_StartTime2.Text = "00";
            OTSim_StartTime3.Text = "00";

            OTSim_EndTime0.Text = "0";
            OTSim_EndTime1.Text = "12";
            OTSim_EndTime2.Text = "00";
            OTSim_EndTime3.Text = "00";

            OTSim_CurTime0.Text = "0";
            OTSim_CurTime1.Text = "08";
            OTSim_CurTime2.Text = "00";
            OTSim_CurTime3.Text = "00";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            TS2Sim_StartTime0.Text = "0";
            TS2Sim_StartTime1.Text = "08";
            TS2Sim_StartTime2.Text = "00";
            TS2Sim_StartTime3.Text = "00";

            TS2Sim_EndTime0.Text = "0";
            TS2Sim_EndTime1.Text = "12";
            TS2Sim_EndTime2.Text = "00";
            TS2Sim_EndTime3.Text = "00";

            TS2Sim_CurTime0.Text = "0";
            TS2Sim_CurTime1.Text = "08";
            TS2Sim_CurTime2.Text = "00";
            TS2Sim_CurTime3.Text = "00";
        }
    }
}
