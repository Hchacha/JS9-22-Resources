using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class AnalysisForm : Form
    {
        public AnalysisForm()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            chart1.Titles.Clear();//清理标题
            chart1.Legends[0].Enabled = false;
            chart1.Series[0].Color = Color.Green;
            chart1.Series[1].Color = Color.Blue;
        }

        int allrow;
        int startrow;
        string[] train = new string[10000];
        string[] station = new string[10000];
        double[] depactual = new double[10000];
        double[] sumdelay = new double[10000];
        string[] inlinetrain = new string[] {"G101", "G103", "G5", "G105", "G143", "G107", "G109",
            "G111", "G113", "G1", "G41", "G115", "G117", "G7", "G119", "G121", "G123", "G125",
            "G411", "G127", "G129", "G131", "G133", "G135", "G137", "G139", "G3", "G5", "G43",
            "G141", "G145", "G11", "G155", "G147", "G149", "G169", "G151", "G13", "G153", "G157",
            "G159", "G15", "G17", "G21", "G102", "G104", "G6", "G106", "G144", "G108", "G110",
            "G112", "G114", "G2", "G42", "G116", "G118", "G8", "G120", "G122", "G124", "G126",
            "G412", "G128", "G130", "G132", "G134", "G136", "G138", "G140", "G4", "G6", "G44",
            "G142", "G146", "G12", "G156", "G148", "G150", "G170", "G152", "G14", "G154", "G158",
            "G60", "G16", "G18", "G22" };//本线车集合


        private void button1_Click(object sender, EventArgs e)
        {

            FileStream fs;//定义文件流变量fs
            string path = @"D:\田祥龙\能力系统研发\OUTPUT" + @"\OT_TimetableStatistics.txt";//文件路径
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);//打开路径对应的文件存入fs
            StreamReader sr;//定义从文件流中读取字符的变量sr
            sr = new StreamReader(fs);//读取的是文件流fs


            string[] contents = File.ReadAllLines(path, Encoding.Default);
            allrow = contents.Length;//一共有多少行


            for (int i = 0; contents[i] != "Actual Timetable vs. Planned Timetable"; i++)
            {
                startrow = i + 6;
            } //这部分是识别"Actual Timetable vs. Planned Timetable"行数加五，该行开始读取到数组中

            List<string[]> arrary = new List<string[]>();//创建类型为string[]的对象集合arrary

            while (!sr.EndOfStream)//只要没有到文件结尾就循环下面的操作
            {
                string pp = sr.ReadLine();//从文件中读取行存入字符串pp
                string[] arr = Regex.Split(pp, "\\s+", RegexOptions.IgnoreCase); //利用split将pp分解成多个元素构成的数组

                arrary.Add(arr);//把数组arr[]放入arrary   

            }


            string[] stationID = new string[allrow - startrow];
            string[] arrplantime = new string[allrow - startrow];
            double[] arrplan = new double[allrow - startrow];
            string[] depplantime = new string[allrow - startrow];
            double[] depplan = new double[allrow - startrow];
            string[] arractualtime = new string[allrow - startrow];
            double[] arractual = new double[allrow - startrow];
            string[] depactualtime = new string[allrow - startrow];
            string[] arrdifftime = new string[allrow - startrow];
            double[] arrdiff = new double[allrow - startrow];
            string[] depdifftime = new string[allrow - startrow];
            double[] depdiff = new double[allrow - startrow];

            //将各列数据分到不同数组中
            for (int i = 0; i < allrow - startrow; i++)
            {
                train[i] = arrary[i + startrow][0];
                stationID[i] = arrary[i + startrow][1];
                arrplantime[i] = arrary[i + startrow][2];
                arrplan[i] = System.Convert.ToDouble(arrary[i + startrow][3]);
                depplantime[i] = arrary[i + startrow][4];
                depplan[i] = System.Convert.ToDouble(arrary[i + startrow][5]);
                arractualtime[i] = arrary[i + startrow][6];
                arractual[i] = System.Convert.ToDouble(arrary[i + startrow][7]);
                depactualtime[i] = arrary[i + startrow][8];
                depactual[i] = System.Convert.ToDouble(arrary[i + startrow][9]);
                arrdifftime[i] = arrary[i + startrow][10];
                arrdiff[i] = System.Convert.ToDouble(arrary[i + startrow][11]);
                depdifftime[i] = arrary[i + startrow][12];
                depdiff[i] = System.Convert.ToDouble(arrary[i + startrow][13]);
                if (arrdiff[i] <= 0) { arrdiff[i] = 0; }
                if (depdiff[i] <= 0) { depdiff[i] = 0; }//延误量为负数，即提前到的车，处理为0

            }

            //把车站编码替换为车站名
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (stationID[i] == "JH01")
                {
                    station[i] = "北京南";
                }
                else if (stationID[i] == "JH02")
                {
                    station[i] = "廊坊";
                }
                else if (stationID[i] == "JH04")
                {
                    station[i] = "天津南";
                }
                else if (stationID[i] == "JH05")
                {
                    station[i] = "沧州西";
                }
                else if (stationID[i] == "JH06")
                {
                    station[i] = "德州东";
                }
                else if (stationID[i] == "STA_01")
                {
                    station[i] = "济南西";
                }
                else if (stationID[i] == "STA_02")
                {
                    station[i] = "泰安";
                }
                else if (stationID[i] == "STA_03")
                {
                    station[i] = "曲阜东";
                }
                else if (stationID[i] == "STA_04")
                {
                    station[i] = "滕州东";
                }
                else if (stationID[i] == "STA_05")
                {
                    station[i] = "枣庄";
                }
                else if (stationID[i] == "STA_06")
                {
                    station[i] = "徐州东";
                }
                else if (stationID[i] == "STA_07")
                {
                    station[i] = "宿州东";
                }
                else if (stationID[i] == "STA_08")
                {
                    station[i] = "蚌埠南";
                }
                else if (stationID[i] == "STA_09")
                {
                    station[i] = "定远";
                }
                else if (stationID[i] == "STA_10")
                {
                    station[i] = "滁州";
                }
                else if (stationID[i] == "STA_11")
                {
                    station[i] = "南京南";
                }
                else if (stationID[i] == "STA_12")
                {
                    station[i] = "镇江南";
                }
                else if (stationID[i] == "STA_13")
                {
                    station[i] = "丹阳北";
                }
                else if (stationID[i] == "STA_14")
                {
                    station[i] = "常州北";
                }
                else if (stationID[i] == "STA_15")
                {
                    station[i] = "无锡东";
                }
                else if (stationID[i] == "STA_16")
                {
                    station[i] = "苏州北";
                }
                else if (stationID[i] == "STA_17")
                {
                    station[i] = "昆山南";
                }
                else if (stationID[i] == "STA_18")
                {
                    station[i] = "上海虹桥";
                }

            }
            for (int i = 0; i < allrow - startrow; i++)
            {
                sumdelay[i] = arrdiff[i] + depdiff[i];//计算各行总延误
            }

            List<string> listString = new List<string>();
            foreach (string eachString in station)
            {
                if (!listString.Contains(eachString))
                    listString.Add(eachString);
            }//将station数组中重复项取出放入listString

            for (int i = 0; i < allrow - startrow; i++)
            {
                if (train[i] != train[i + 1])
                {
                    comboBox1.Items.Add(train[i]);
                    comboBox2.Items.Add(train[i]);
                }
            }
            for (int i = 0; i < 23; i++)
            {
                comboBox3.Items.Add(listString[i]);
                comboBox4.Items.Add(listString[i]);
                comboBox5.Items.Add(listString[i]);
                comboBox6.Items.Add(listString[i]);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }
            chart1.Legends[0].Enabled = false;

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "所有车次所有车站不同时段延误图";

            chart1.ChartAreas[0].AxisX.Title = "时间段";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.Column;

            double[] allsumdelay = new double[24];
            for (int i = 0; i < allrow - startrow; i++)
            {
                for (int b = 0; b < 24; b++)
                {
                    if (3600 * b < depactual[i] & depactual[i] <= (b + 1) * 3600)
                    {
                        allsumdelay[b] = sumdelay[i] + allsumdelay[b];
                    }
                }
            }
            for (int b = 0; b < 24; b++)
            {
                chart1.Series[0].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay[b]);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }
            chart1.Legends[0].Enabled = true;//显示图例

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "本线跨线车所有车站不同时段延误图";

            chart1.ChartAreas[0].AxisX.Title = "时间段";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.StackedColumn;
            chart1.Series[1].ChartType = SeriesChartType.StackedColumn;
            chart1.Series[0].Name = "本线车";
            chart1.Series[1].Name = "跨线车";

            double[] allsumdelay1 = new double[24];
            double[] allsumdelay2 = new double[24];


            for (int i = 0; i < allrow - startrow; i++)
            {
                for (int b = 0; b < 24; b++)
                {
                    if (3600 * b < depactual[i] & depactual[i] <= (b + 1) * 3600)
                    {
                        bool exists = ((IList)inlinetrain).Contains(train[i]);//判断这一行是不是本线车
                        if (exists)
                        {
                            allsumdelay1[b] = sumdelay[i] + allsumdelay1[b];//本线车延误加到allsumdelay1中
                        }
                        if (!exists)
                        {
                            allsumdelay2[b] = sumdelay[i] + allsumdelay2[b];
                        }

                    }
                }
            }
            for (int b = 0; b < 24; b++)
            {
                chart1.Series[0].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay1[b]);
                chart1.Series[1].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay2[b]);

            }

            foreach (Series s in chart1.Series)
            {
                s["StackedGroupName"] = s.Name;
            }//多柱与堆积

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "特定车次所有车站不同时段延误图";

            chart1.Legends[0].Enabled = false;
            chart1.ChartAreas[0].AxisX.Title = "时间段";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.Column;


            string a = comboBox1.SelectedItem.ToString();
            double[] allsumdelay = new double[24];
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (train[i] == a)
                {
                    for (int b = 0; b < 24; b++)
                    {
                        if (3600 * b < depactual[i] & depactual[i] <= (b + 1) * 3600)
                        {
                            allsumdelay[b] = sumdelay[i] + allsumdelay[b];
                        }
                    }
                }

            }
            for (int b = 0; b < 24; b++)
            {
                chart1.Series[0].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay[b]);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "特定车次不同车站所有时段延误图";

            chart1.Legends[0].Enabled = false;
            chart1.ChartAreas[0].AxisX.Title = "车站";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.Column;


            string a = comboBox2.SelectedItem.ToString();
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (train[i] == a)
                {
                    chart1.Series[0].Points.AddXY(station[i], sumdelay[i]);//注意：此处默认每辆车每个站只经过一次
                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "所有车次特定车站不同时段延误图";

            chart1.Legends[0].Enabled = false;
            chart1.ChartAreas[0].AxisX.Title = "时间段";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.Column;

            string a = comboBox3.SelectedItem.ToString();
            double[] allsumdelay = new double[24];
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (station[i] == a)
                {
                    for (int b = 0; b < 24; b++)
                    {
                        if (3600 * b < depactual[i] & depactual[i] <= (b + 1) * 3600)
                        {
                            allsumdelay[b] = sumdelay[i] + allsumdelay[b];
                        }
                    }
                }

            }
            for (int b = 0; b < 24; b++)
            {
                chart1.Series[0].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay[b]);
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "不同车次特定车站所有时段延误图";

            chart1.Legends[0].Enabled = false;
            chart1.ChartAreas[0].AxisX.Title = "车次";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.Column;

            string a = comboBox4.SelectedItem.ToString();
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (station[i] == a)
                {
                    chart1.Series[0].Points.AddXY(train[i], sumdelay[i]);//注意：此处默认每辆车每个站只经过一次
                }
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "本线跨线车特定车站不同时段延误图";

            chart1.Legends[0].Enabled = true;
            chart1.ChartAreas[0].AxisX.Title = "时间段";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.StackedColumn;
            chart1.Series[1].ChartType = SeriesChartType.StackedColumn;
            chart1.Series[0].Name = "本线车";
            chart1.Series[1].Name = "跨线车";

            string a = comboBox5.SelectedItem.ToString();
            double[] allsumdelay1 = new double[24];
            double[] allsumdelay2 = new double[24];
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (station[i] == a)
                {
                    for (int b = 0; b < 24; b++)
                    {
                        if (3600 * b < depactual[i] & depactual[i] <= (b + 1) * 3600)
                        {
                            bool exists = ((IList)inlinetrain).Contains(train[i]);//判断这一行是不是本线车
                            if (exists)
                            {
                                allsumdelay1[b] = sumdelay[i] + allsumdelay1[b];//本线车延误加到allsumdelay1中
                            }
                            if (!exists)
                            {
                                allsumdelay2[b] = sumdelay[i] + allsumdelay2[b];
                            }
                        }
                    }
                }

            }
            for (int b = 0; b < 24; b++)
            {
                chart1.Series[0].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay1[b]);
                chart1.Series[1].Points.AddXY(b + ":00-" + (b + 1) + ":00", allsumdelay2[b]);
            }
            foreach (Series s in chart1.Series)
            {
                s["StackedGroupName"] = s.Name;
            }//多柱与堆积


        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            chart1.Titles.Clear();//清理标题
            Title title = new Title();
            chart1.Titles.Add(title);
            chart1.Titles[0].Text = "本线跨线车特定车站不同车次延误图";

            chart1.Legends[0].Enabled = true;
            chart1.ChartAreas[0].AxisX.Title = "车次";//横坐标轴标题
            chart1.Series[0].ChartType = SeriesChartType.Column;

            chart1.Series[0].Name = "本线车";
            chart1.Series[1].Name = "跨线车";



            string a = comboBox6.SelectedItem.ToString();
            int inline = 0;
            int outline = 0;
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (station[i] == a)
                {
                    bool exists = ((IList)inlinetrain).Contains(train[i]);//判断这一行是不是本线车
                    if (exists)
                    {
                        chart1.Series[0].Points.AddXY(train[i], sumdelay[i]);
                        inline++;

                    }


                }
            }
            for (int i = 0; i < allrow - startrow; i++)
            {
                if (station[i] == a)
                {
                    bool exists = ((IList)inlinetrain).Contains(train[i]);//判断这一行是不是本线车
                    if (!exists)
                    {
                        chart1.Series[0].Points.AddXY(train[i], sumdelay[i]);
                        outline++;
                    }

                }
            }
            for (int i = inline; i < (inline + outline); i++)
            {
                chart1.Series[0].Points[i].Color = Color.Blue;
            }

        }
    }
}
