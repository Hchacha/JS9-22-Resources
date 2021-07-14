using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Collections;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public class SectionCapCal
    {
        //数据地址
        public string DataPath;//数据库地址
        public string TimeTableName;//运行图表的名称
        public string StationInterName;//车站间隔时间表的名称
        public string LineDataName;//线路信息表的名称
        public string SectionDataName;//区段信息表的名称

        //数据表
        public DataSet AllDataTable;//数据集
        public OleDbConnection DBConnection;//数据连接
        public OleDbDataAdapter DataAdapter;//数据接口


        public DataTable TimeTable;//全部运行图
        public DataTable SecTimeTable;//区段运行图
        public DataTable StationInterTab;//车站间隔时间表
        public DataTable LineDataTable;//线路信息表
        public DataTable SectionDataTable;//区段信息表

        //计算参数
        public int WindowTime;//天窗时间 分钟
        public int StdTravalTime;//标准列车区段旅行时间
        public int StdInterval;//标准列车最短间隔时间
        public List<String> SectionStations = new List<string>();//要计算的区段包含的车站，按顺序排列
        public string Direction;//区段方向 上行、下行

        //计算结果
        public double ParCapacity;//平行运行图通过能力

        public double CapacityDeduction;//扣除系数法通过能力
        public string UtilizationDeduction;//扣除系数法利用率

        public double CapacityGroupDeduction;//组合扣除法通过能力
        public string UtilizationGroupDeduction;//组合扣除系数法利用率
        public int GroupCount;//分成的列车组数


        public double CapacityUIC406;//UIC406法通过能力
        public string UtilizationUIC406;//UIC406法利用率
        public int GroupCountUIC;//分成的列车组数



        public SectionCapCal()
        {

        }


        //将计算结果更新到数据库中
        public void ResultUpdate(int sectionIndex, string calMethod)
        {

            OleDbCommand dbCommand = new OleDbCommand();
            dbCommand.Connection = this.DBConnection;
            dbCommand.CommandText = "SELECT * FROM " + this.SectionDataName;
            dbCommand.CommandType = CommandType.Text;

            //3.创建数据适配器
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(dbCommand.CommandText, this.DBConnection);

            //4.创建数据表
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds, this.SectionDataName);
            DataTable sectionData = ds.Tables[0];

            DataRow[] thisSection = sectionData.Select(String.Format("区段编号 = '{0}'", sectionIndex));
            switch (calMethod)
            {

                case "扣除系数法":
                    {
                        thisSection[0]["通过能力"] = this.CapacityDeduction;
                        thisSection[0]["能力利用率"] = this.UtilizationDeduction;
                        break;
                    }
                case "组合扣除法":
                    {
                        thisSection[0]["通过能力"] = this.CapacityGroupDeduction;
                        thisSection[0]["能力利用率"] = this.UtilizationGroupDeduction;
                        break;
                    }
                case "压缩加密法":
                    {
                        thisSection[0]["通过能力"] = this.CapacityUIC406;
                        thisSection[0]["能力利用率"] = this.UtilizationUIC406;
                        break;
                    }

                default:
                    break;
            }
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapter);


            dataAdapter.Update(sectionData);


        }

        public void SectionParameterSet(int sectionIndex)

        {
            this.WindowTime = Convert.ToInt32(SectionDataTable.Rows[sectionIndex - 1]["天窗时间"].ToString());//区段的天窗时间;
            string stdtravaltime = SectionDataTable.Rows[sectionIndex - 1]["区段运行时间"].ToString();
            string[] sArr = stdtravaltime.Split(':');
            int hh = Convert.ToInt32(sArr[0]);
            int mm = Convert.ToInt32(sArr[1]);
            int ss = Convert.ToInt32(sArr[2]);
            int totalsec = hh * 3600 + mm * 60 + ss;
            this.StdTravalTime = totalsec;//区段的天窗时间;
            this.StdInterval = Convert.ToInt32(SectionDataTable.Rows[sectionIndex - 1]["区段间隔时间"].ToString());//区段的间隔时间;

            this.Direction = SectionDataTable.Rows[sectionIndex - 1]["区段方向"].ToString();
            this.SectionStations.Clear();
            //获取区段车站
            int stationNum = Convert.ToInt32(SectionDataTable.Rows[sectionIndex - 1]["车站数量"].ToString());//区段的车站数量
            string startStation = SectionDataTable.Rows[sectionIndex - 1]["起始站"].ToString();//起始车站
            string endStation = SectionDataTable.Rows[sectionIndex - 1]["终点站"].ToString();//终点车站
            int startStationIndex = 0;
            int endStationIndex = 0;
            for (int i = 0; i < LineDataTable.Rows.Count; i++)
            {
                if (LineDataTable.Rows[i]["车站"].ToString() == startStation)
                {
                    startStationIndex = i;
                }
                if (LineDataTable.Rows[i]["车站"].ToString() == endStation)
                {
                    endStationIndex = i;
                }
            }
            if (startStationIndex < endStationIndex)
            {
                for (int j = startStationIndex; j <= endStationIndex; j++)
                {
                    this.SectionStations.Add(LineDataTable.Rows[j]["车站"].ToString());
                }

            }
            else
            {
                for (int j = startStationIndex; j >= endStationIndex; j--)
                {
                    this.SectionStations.Add(LineDataTable.Rows[j]["车站"].ToString());
                }

            }

            //形成区段运行图
            this.SecTimeTable = TimeTable.Copy();
            this.SecTimeTable.TableName = "区段运行图";
            //1.先操作列，去掉无关车站
            for (int i = (this.SecTimeTable.Columns.Count - 1); i > 4; i--)
            {
                int k = 0;
                for (int j = 0; j < this.SectionStations.Count; j++)
                {

                    if (this.SecTimeTable.Columns[i].ToString().Contains(this.SectionStations[j].ToString()) == true)
                    {
                        k++;
                    }
                }
                if (k == 0)
                {
                    this.SecTimeTable.Columns.RemoveAt(i);
                }
            }
            //2.再操作行，去掉无关行
            //3个条件，高速、方向、无-
            for (int i = (this.SecTimeTable.Rows.Count - 1); i >= 0; i--)
            {
                //是否是高速
                if (this.SecTimeTable.Rows[i]["列车种类"].ToString() != "高速")
                {
                    this.SecTimeTable.Rows.RemoveAt(i);
                    continue;
                }
                //是否是同向
                if (this.SecTimeTable.Rows[i]["方向"].ToString() != this.Direction)
                {
                    this.SecTimeTable.Rows.RemoveAt(i);
                    continue;
                }
                //是否有-
                for (int j = 5; j < this.SecTimeTable.Columns.Count; j++)
                {
                    if (this.SecTimeTable.Rows[i][j].ToString() == "-")
                    {
                        this.SecTimeTable.Rows.RemoveAt(i);
                        break;
                    }
                    //时分秒转换成秒
                    else
                    {

                        string[] sArray = this.SecTimeTable.Rows[i][j].ToString().Split(':');
                        int hour = Convert.ToInt32(sArray[0]);
                        int min = Convert.ToInt32(sArray[1]);
                        int sec = Convert.ToInt32(sArray[2]);
                        int totalSec = hour * 3600 + min * 60 + sec;
                        this.SecTimeTable.Rows[i][j] = totalSec;
                    }
                }
            }
            ParallelCap();



        }

        public void DataManage(string dataBasePath, string lineData, string sectionData, string timeTable, string stationInterval)
        {
            this.DataPath = dataBasePath;
            this.LineDataName = lineData;
            this.SectionDataName = sectionData;
            this.TimeTableName = timeTable;
            this.StationInterName = stationInterval;


            //导入数据
            //1.连接数据库
            string conStr = @"Provider = Microsoft.Jet.OLEDB.4.0; Data Source = " + this.DataPath;
            try
            {
                this.DBConnection = new OleDbConnection(conStr);
                this.DBConnection.Open();
            }
            catch (Exception)
            {
                
                MessageBox.Show("请选择数据库") ;
                return;
                
            }
            finally
            {
                DBConnection.Close();
            }



            //读取线路数据
            //2.写命令
            OleDbCommand dbCommand = new OleDbCommand();
            dbCommand.Connection = this.DBConnection;
            dbCommand.CommandText = "SELECT * FROM " + this.LineDataName;
            dbCommand.CommandType = CommandType.Text;

            //3.创建数据适配器
            this.DataAdapter = new OleDbDataAdapter(dbCommand.CommandText, this.DBConnection);

            //4.创建数据表
            this.AllDataTable = new DataSet();
            this.DataAdapter.Fill(this.AllDataTable, this.LineDataName);
            this.LineDataTable = this.AllDataTable.Tables[0];

            //(1)读取区段数据
            dbCommand.CommandText = "SELECT * FROM " + this.SectionDataName;
            this.DataAdapter = new OleDbDataAdapter(dbCommand);
            this.DataAdapter.Fill(this.AllDataTable, this.SectionDataName); //将适配器中的内容填充到dataset的thetable表中， thetable同时被建立

            this.SectionDataTable = this.AllDataTable.Tables[1];


            //(2)读取运行图数据
            dbCommand.CommandText = "SELECT * FROM " + this.TimeTableName;
            this.DataAdapter = new OleDbDataAdapter(dbCommand);
            this.DataAdapter.Fill(this.AllDataTable, this.TimeTableName); //将适配器中的内容填充到dataset的thetable表中， thetable同时被建立

            this.TimeTable = this.AllDataTable.Tables[2];

            //(3)读取车站间隔时间数据
            dbCommand.CommandText = "SELECT * FROM " + this.StationInterName;
            this.DataAdapter = new OleDbDataAdapter(dbCommand);
            this.DataAdapter.Fill(this.AllDataTable, this.StationInterName); //将适配器中的内容填充到dataset的thetable表中， thetable同时被建立

            this.StationInterTab = this.AllDataTable.Tables[3];


        }
        //平行运行图能力计算
        public void ParallelCap()
        {
            double parallelCapacity = (1440 * 60 - this.WindowTime * 60 - this.StdTravalTime) / this.StdInterval;
            this.ParCapacity = parallelCapacity;
        }




        //传统扣除系数法
        public void DeductCoefficient()
        {

            string firstStation = SectionStations[0];//第一个车站
            string lastStation = SectionStations[SectionStations.Count - 1];//最后一个车站
            int trainCount = this.SecTimeTable.Rows.Count;//列车数量
            double totalDeduction = 0;//扣除能力
            for (int i = 0; i < trainCount; i++)
            {
                int arrivalTime = Convert.ToInt32(SecTimeTable.Rows[i][lastStation + '到'].ToString());
                int departTime = Convert.ToInt32(SecTimeTable.Rows[i][firstStation + '发'].ToString());
                double singelDeduction = (arrivalTime - departTime - this.StdTravalTime + this.StdInterval) / this.StdInterval;
                totalDeduction = totalDeduction + singelDeduction;
            }
            double utilization = totalDeduction / this.ParCapacity;//能力利用率
            double carryCapacity = (trainCount * this.ParCapacity) / totalDeduction;//通过能力

            this.UtilizationDeduction = utilization.ToString("P");
            this.CapacityDeduction = carryCapacity;
        }


        public void GroupDeduction()
        {
            //数据准备
            DataTable secTimetable = this.SecTimeTable.Copy();
            DataTable staInterTab = this.StationInterTab.Copy();

            int trainCount = secTimetable.Rows.Count;//列车数量
            string firstStation = this.SectionStations[0];//第一个车站
            string lastStation = this.SectionStations[SectionStations.Count - 1];//最后一个车站



            DataRow[] intervalFirstStation = staInterTab.Select(String.Format("车站 = '{0}' and 方向 = '{1}'", firstStation, this.Direction));//起点车站间隔时间
            DataRow[] intervalLastStation = staInterTab.Select(String.Format("车站 = '{0}' and 方向 = '{1}'", lastStation, this.Direction));//终点车站间隔时间
            int IntervalLastAA = Convert.ToInt32(intervalLastStation[0]["到到"].ToString());


            //按始发站到达时间排序
            DataView dv = secTimetable.DefaultView;//获取表视图
            dv.Sort = string.Format(firstStation + '到');
            secTimetable = dv.ToTable();//转为表
            //1.计算各个列车的T占
            int[,] T_zhan = new int[trainCount, 2];
            for (int i = 0; i < trainCount; i++)
            {
                T_zhan[i, 0] = Convert.ToInt32(secTimetable.Rows[i][firstStation + '发'].ToString());
                int travalTime_i = Convert.ToInt32(secTimetable.Rows[i][lastStation + '到'].ToString()) - Convert.ToInt32(secTimetable.Rows[i][firstStation + '发'].ToString());
                int T_zhan_i = travalTime_i + IntervalLastAA - this.StdTravalTime;
                T_zhan[i, 1] = T_zhan[i, 0] + T_zhan_i;
            }

            //2.对列车进行分组
            List<String> matchedTrains = new List<string>();//已经匹配到组的列车集合
            List<List<String>> trainGroups = new List<List<string>>();//划分的结果

            for (int i = 0; i < trainCount; i++)
            {
                if (matchedTrains.Contains(secTimetable.Rows[i]["车次"].ToString()))
                {
                    continue;
                }
                List<String> uncheckedTrains = new List<string> { secTimetable.Rows[i]["车次"].ToString() };//未检查过的列车集合
                List<String> checkedTrains = new List<string>();//检查过的列车集合

                while (uncheckedTrains.Count > 0)
                {
                    for (int j = 0; j < trainCount; j++)
                    {
                        //bool ischecked= checkedTrains.Contains(secTimetable.Rows[j]["车次"].ToString());&& (ischecked == false)
                        if (uncheckedTrains.Contains(secTimetable.Rows[j]["车次"].ToString()))
                        {
                            List<String> thisGroup = new List<string>();//与此列车有重复占用关系的列车集合
                            for (int k = j + 1; k < trainCount; k++)
                            {
                                if (T_zhan[j, 1] > T_zhan[k, 0])
                                {
                                    thisGroup.Add(secTimetable.Rows[k]["车次"].ToString());
                                }
                            }
                            checkedTrains.Add(secTimetable.Rows[j]["车次"].ToString());
                            uncheckedTrains.Remove(secTimetable.Rows[j]["车次"].ToString());
                            uncheckedTrains = uncheckedTrains.Union(thisGroup).ToList<string>();
                            uncheckedTrains = uncheckedTrains.Except(checkedTrains).ToList<string>();
                            uncheckedTrains = uncheckedTrains.Except(matchedTrains).ToList<string>();
                            break;
                        }

                    }

                }
                matchedTrains = matchedTrains.Union(checkedTrains).ToList<string>();
                trainGroups.Add(checkedTrains);

            }

            //3.计算组合扣除量
            double totalDeductionG = 0;//组合扣除法的总扣除量
            for (int i = 0; i < trainGroups.Count; i++)
            {
                List<string> trainGroup = trainGroups[i];
                int earlyestDepTime = 86400;//最早列车的发车时间
                int latestDepTime = 0;//最后列车的发车时间
                int lastZhan = 0;//最后列车的占用时间
                int groupZhan = 0;//此列车组的占用时间
                double groupDeduction = 0;//此列车组的扣除系数 

                //寻找最早和最晚的发车时间
                for (int j = 0; j < trainGroup.Count; j++)
                {
                    DataRow[] thisTrain = secTimetable.Select(string.Format("车次 = '{0}'", trainGroup[j]));
                    int thisDepTime = Convert.ToInt32(thisTrain[0][firstStation + "发"].ToString());
                    int thisArrTime = Convert.ToInt32(thisTrain[0][lastStation + "到"].ToString());

                    if (thisDepTime < earlyestDepTime)
                    {
                        earlyestDepTime = thisDepTime;
                    }
                    if (thisDepTime > latestDepTime)
                    {
                        latestDepTime = thisDepTime;
                    }
                }
                //寻找最晚列车的占用时间
                for (int k = 0; k < T_zhan.GetLength(0); k++)
                {
                    if (T_zhan[k, 0] == latestDepTime)
                    {
                        lastZhan = T_zhan[k, 1] - T_zhan[k, 0];
                    }
                }
                groupZhan = latestDepTime + lastZhan - earlyestDepTime;
                groupDeduction = (double)groupZhan / (double)this.StdInterval;
                totalDeductionG = totalDeductionG + groupDeduction;
            }
            //4.计算
            double utilization = totalDeductionG / this.ParCapacity;//能力利用率
            double carryCapacity = (trainCount * this.ParCapacity) / totalDeductionG;//通过能力

            this.CapacityGroupDeduction = carryCapacity;
            this.UtilizationGroupDeduction = utilization.ToString("P");
            this.GroupCount = trainGroups.Count;

        }

        public void UIC406()
        {
            //数据准备
            DataTable secTimetable = this.SecTimeTable.Copy();
            DataTable staInterTab = this.StationInterTab.Copy();
            int trainCount = secTimetable.Rows.Count;//列车数量
            string firstStation = SectionStations[0];//第一个车站
            string lastStation = SectionStations[SectionStations.Count - 1];//最后一个车站
            DataRow[] intervalFirstStation = staInterTab.Select(String.Format("车站 = '{0}' and 方向 = '{1}'", firstStation, this.Direction));//起点车站间隔时间
            DataRow[] intervalLastStation = staInterTab.Select(String.Format("车站 = '{0}' and 方向 = '{1}'", firstStation, this.Direction));//终点车站间隔时间
            //按始发站到达时间排序
            DataView dv = secTimetable.DefaultView;//获取表视图
            dv.Sort = string.Format(firstStation + '到');
            secTimetable = dv.ToTable();//转为表

            //1.对列车进行分组
            List<String> matchedTrains = new List<string>();//已经匹配到组的列车集合
            List<List<String>> trainGroups = new List<List<string>>();//划分的结果

            //对于所有列车
            for (int i = 0; i < trainCount; i++)
            {
                string thisTrain = secTimetable.Rows[i]["车次"].ToString();
                //如果还没有匹配到组
                if (matchedTrains.Contains(thisTrain))
                {
                    continue;
                }
                List<String> uncheckedTrains = new List<string> { thisTrain };//未检查过的列车集合
                List<String> checkedTrains = new List<string>();//检查过的列车集合


                while (uncheckedTrains.Count > 0)
                {
                    for (int j = 0; j < trainCount; j++)
                    {
                        //bool ischecked= checkedTrains.Contains(secTimetable.Rows[j]["车次"].ToString());&& (ischecked == false)
                        if (uncheckedTrains.Contains(secTimetable.Rows[j]["车次"].ToString()))
                        {
                            List<String> neibTrains = new List<string>();//与此列车紧邻的列车集合
                            neibTrains = GetNeibour(secTimetable.Rows[j]["车次"].ToString());

                            checkedTrains.Add(secTimetable.Rows[j]["车次"].ToString());
                            uncheckedTrains.Remove(secTimetable.Rows[j]["车次"].ToString());
                            uncheckedTrains = uncheckedTrains.Union(neibTrains).ToList<string>();
                            uncheckedTrains = uncheckedTrains.Except(checkedTrains).ToList<string>();
                            uncheckedTrains = uncheckedTrains.Except(matchedTrains).ToList<string>();
                            break;
                        }

                    }

                }
                matchedTrains = matchedTrains.Union(checkedTrains).ToList<string>();
                trainGroups.Add(checkedTrains);

            }
            int totaltrain = 0;
            for (int i = 0; i < trainGroups.Count; i++)
            {
                totaltrain = totaltrain + trainGroups[i].Count;
            }

            //2.对运行图进行压缩
            DataTable compressedTable = secTimetable.Clone();//创建一个空表
            //2.1 首先根据天窗时间移动第一组
            int startTime = this.WindowTime * 60;
            List<String> firstGroup = trainGroups[0];
            Dictionary<string, int> firstGroupTimes = new Dictionary<string, int>();//一个列车群的上下左右4个时间 注意上下行
            firstGroupTimes.Add("fdTime", -1);//第一个出发
            firstGroupTimes.Add("ldTime", -1);//最后一个出发
            firstGroupTimes.Add("faTime", -1);//第一个到达
            firstGroupTimes.Add("laTime", -1);//最后一个出发
            //(1)寻找起始站的最早、最晚发车时间和终到站最早、最晚达到时间
            List<int> firstStationDeps = new List<int>();
            List<int> lastStationArrs = new List<int>();
            for (int i = 0; i < firstGroup.Count; i++)
            {
                DataRow[] thisTrain = secTimetable.Select(string.Format("车次 = '{0}'", firstGroup[i]));
                int thisDepTime = Convert.ToInt32(thisTrain[0][firstStation + "发"].ToString());
                int thisArrTime = Convert.ToInt32(thisTrain[0][lastStation + "到"].ToString());
                firstStationDeps.Add(thisDepTime);
                lastStationArrs.Add(thisArrTime);
            }
            firstGroupTimes["fdTime"] = firstStationDeps.Min();
            firstGroupTimes["ldTime"] = firstStationDeps.Max();
            firstGroupTimes["faTime"] = lastStationArrs.Min();
            firstGroupTimes["laTime"] = lastStationArrs.Max();
            int firstGroupShift = firstGroupTimes["fdTime"] - startTime;
            //(2)移动第一组列车到天窗开始时间
            for (int i = 0; i < firstGroup.Count; i++)
            {
                DataRow[] thisTrain = secTimetable.Select(string.Format("车次 = '{0}'", firstGroup[i]));
                for (int j = 0; j < SectionStations.Count; j++)
                {
                    thisTrain[0][SectionStations[j] + "到"] = Convert.ToInt32(thisTrain[0][SectionStations[j] + "到"]) - firstGroupShift;
                    thisTrain[0][SectionStations[j] + "发"] = Convert.ToInt32(thisTrain[0][SectionStations[j] + "发"]) - firstGroupShift;
                }
                compressedTable.ImportRow(thisTrain[0]);

            }
            firstGroupTimes["fdTime"] = firstGroupTimes["fdTime"] - firstGroupShift;
            firstGroupTimes["ldTime"] = firstGroupTimes["ldTime"] - firstGroupShift;
            firstGroupTimes["faTime"] = firstGroupTimes["faTime"] - firstGroupShift;
            firstGroupTimes["laTime"] = firstGroupTimes["laTime"] - firstGroupShift;

            //2.2 移动其他列车
            Dictionary<string, int> lastGroupTimes = new Dictionary<string, int>(firstGroupTimes);//上一个列车组合

            //对于所有的列车组合
            for (int i = 1; i < trainGroups.Count; i++)
            {
                List<String> thisGroup = trainGroups[i];
                Dictionary<string, int> thisGroupTimes = new Dictionary<string, int>();//一个列车群的上下左右4个时间 注意上下行
                thisGroupTimes.Add("fdTime", -1);//第一个出发
                thisGroupTimes.Add("ldTime", -1);//最后一个出发
                thisGroupTimes.Add("faTime", -1);//第一个到达
                thisGroupTimes.Add("laTime", -1);//最后一个出发
                List<int> firStationDeps = new List<int>();
                List<int> lasStationArrs = new List<int>();
                //对于组合内所有的列车
                for (int j = 0; j < thisGroup.Count; j++)
                {
                    DataRow[] thisTrain = secTimetable.Select(string.Format("车次 = '{0}'", thisGroup[j]));
                    int thisDepTime = Convert.ToInt32(thisTrain[0][firstStation + "发"].ToString());
                    int thisArrTime = Convert.ToInt32(thisTrain[0][lastStation + "到"].ToString());
                    firStationDeps.Add(thisDepTime);
                    lasStationArrs.Add(thisArrTime);
                }
                thisGroupTimes["fdTime"] = firStationDeps.Min();
                thisGroupTimes["ldTime"] = firStationDeps.Max();
                thisGroupTimes["faTime"] = lasStationArrs.Min();
                thisGroupTimes["laTime"] = lasStationArrs.Max();
                int depDifference = thisGroupTimes["fdTime"] - lastGroupTimes["ldTime"] - Convert.ToInt32(intervalFirstStation[0]["发发"].ToString());
                int arrDifference = thisGroupTimes["faTime"] - lastGroupTimes["laTime"] - Convert.ToInt32(intervalLastStation[0]["到到"].ToString());
                int shiftValue = Math.Min(depDifference, arrDifference);
                for (int j = 0; j < thisGroup.Count; j++)
                {
                    DataRow[] thisTrain = secTimetable.Select(string.Format("车次 = '{0}'", thisGroup[j]));
                    for (int k = 0; k < SectionStations.Count; k++)
                    {
                        thisTrain[0][SectionStations[k] + "到"] = Convert.ToInt32(thisTrain[0][SectionStations[k] + "到"]) - shiftValue;
                        thisTrain[0][SectionStations[k] + "发"] = Convert.ToInt32(thisTrain[0][SectionStations[k] + "发"]) - shiftValue;
                    }
                    compressedTable.ImportRow(thisTrain[0]);
                }
                thisGroupTimes["fdTime"] = thisGroupTimes["fdTime"] - shiftValue;
                thisGroupTimes["ldTime"] = thisGroupTimes["ldTime"] - shiftValue;
                thisGroupTimes["faTime"] = thisGroupTimes["faTime"] - shiftValue;
                thisGroupTimes["laTime"] = thisGroupTimes["laTime"] - shiftValue;

                lastGroupTimes["fdTime"] = thisGroupTimes["fdTime"];
                lastGroupTimes["ldTime"] = thisGroupTimes["ldTime"];
                lastGroupTimes["faTime"] = thisGroupTimes["faTime"];
                lastGroupTimes["laTime"] = thisGroupTimes["laTime"];

            }

            //3.通过能力计算compressedTable
            int firstDepTime = Convert.ToInt32(compressedTable.Rows[0][firstStation + "发"].ToString());
            int lastDepTime = Convert.ToInt32(compressedTable.Rows[compressedTable.Rows.Count - 1][firstStation + "发"].ToString());
            int lastArrTime = Convert.ToInt32(compressedTable.Rows[compressedTable.Rows.Count - 1][lastStation + "到"].ToString());
            int availableTime = 1440 * 60 - this.WindowTime * 60;
            int T_zhan = lastArrTime + Convert.ToInt32(intervalLastStation[0]["到到"].ToString()) - this.StdTravalTime - firstDepTime;

            double utilization = (double)T_zhan / (double)availableTime;//能力利用率
            double carryCapacity = (trainCount * availableTime) / T_zhan;//通过能力

            this.CapacityUIC406 = carryCapacity;
            this.UtilizationUIC406 = utilization.ToString("P");
            this.GroupCountUIC = trainGroups.Count;
            compressedTable.Dispose();
            secTimetable.Dispose();

        }
        //UIC406寻找邻居列车的函数
        public List<String> GetNeibour(string thisTrain)
        {
            DataTable secTimetable = this.SecTimeTable.Copy();
            DataTable staIntervalTab = this.StationInterTab.Copy();
            int trainCount = this.SecTimeTable.Rows.Count;
            List<String> neibourTrain = new List<string>();

            //对于所有车站来说
            for (int i = 0; i < this.SectionStations.Count; i++)
            {
                string thisStation = SectionStations[i];
                DataRow[] staInterval = staIntervalTab.Select(String.Format("车站 = '{0}' and 方向 = '{1}'", thisStation, this.Direction));
                int fafa = Convert.ToInt32(staInterval[0]["发发"].ToString());
                int fatong = Convert.ToInt32(staInterval[0]["发通"].ToString());
                int fadao = Convert.ToInt32(staInterval[0]["发到"].ToString());
                int tongfa = Convert.ToInt32(staInterval[0]["通发"].ToString());
                int tongtong = Convert.ToInt32(staInterval[0]["通通"].ToString());
                int tongdao = Convert.ToInt32(staInterval[0]["通到"].ToString());
                int daofa = Convert.ToInt32(staInterval[0]["到发"].ToString());
                int daotong = Convert.ToInt32(staInterval[0]["到通"].ToString());
                int daodao = Convert.ToInt32(staInterval[0]["到到"].ToString());

                string thisTrainType = GetTrainType(thisTrain, thisStation);//列车作业类型
                string leftTrain = null;//前列车车次
                string rightTrain = null;//后列车车次
                //按照此车站的到达时间进行列车排序
                DataView dv = secTimetable.DefaultView;//获取表视图
                dv.Sort = string.Format(thisStation + '到');
                secTimetable = dv.ToTable();//转为表
                int thisTrainIndex = 0;
                int leftTrainIndex = 0;//前列车序号
                int rightTrainIndex = 0;//后列车序号

                //1.寻找在此车站相邻的所有列车
                for (int j = 0; j < trainCount; j++)
                {

                    for (int k = 0; k < trainCount; k++)
                    {
                        if (secTimetable.Rows[k]["车次"].ToString() == thisTrain)
                        {
                            thisTrainIndex = k;
                            leftTrainIndex = k - 1;//前列车序号
                            rightTrainIndex = k + 1;//后列车序号
                            //寻找左右列车车次
                            if (leftTrainIndex >= 0 && leftTrainIndex < trainCount)
                            {
                                leftTrain = secTimetable.Rows[leftTrainIndex]["车次"].ToString();
                            }
                            if (rightTrainIndex >= 0 && rightTrainIndex < trainCount)
                            {
                                rightTrain = secTimetable.Rows[rightTrainIndex]["车次"].ToString();
                            }
                        }

                    }

                }
                //2.判断列车是否属于紧接续
                int thisTrainArr = Convert.ToInt32(secTimetable.Rows[thisTrainIndex][thisStation + "到"].ToString());
                int thisTrainDep = Convert.ToInt32(secTimetable.Rows[thisTrainIndex][thisStation + "发"].ToString());
                //2.1 先判断左列车
                if (leftTrain != null)
                {
                    int leftTrainArr = Convert.ToInt32(secTimetable.Rows[leftTrainIndex][thisStation + "到"].ToString());
                    int leftTrainDep = Convert.ToInt32(secTimetable.Rows[leftTrainIndex][thisStation + "发"].ToString());
                    string leftTrainType = GetTrainType(thisTrain, thisStation);//左列车作业类型
                    switch (thisTrainType)// 始发 终到 通过 停站
                    {
                        case "始发":
                            if (leftTrainType == "始发")
                            {
                                if (thisTrainDep - leftTrainDep <= fafa)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "通过")
                            {
                                if (thisTrainDep - leftTrainDep <= tongfa)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "停站")
                            {
                                if (Math.Abs(thisTrainDep - leftTrainDep) <= fafa)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            break;

                        case "终到":
                            if (leftTrainType == "终到")
                            {
                                if (thisTrainArr - leftTrainArr <= daodao)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "通过")
                            {
                                if (thisTrainArr - leftTrainArr <= tongdao)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "停站")
                            {
                                if (thisTrainArr - leftTrainArr <= daodao)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            break;

                        case "通过":
                            if (leftTrainType == "始发")
                            {
                                if (thisTrainDep - leftTrainDep <= fatong)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "通过")
                            {
                                if (thisTrainDep - leftTrainDep <= tongtong)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "停站")
                            {
                                if ((thisTrainDep >= leftTrainArr) && (thisTrainDep <= leftTrainDep))
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                                if (thisTrainDep - leftTrainArr <= daotong)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                                if (leftTrainDep - thisTrainDep <= tongfa)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "终到")
                            {
                                if (thisTrainArr - leftTrainArr <= daotong)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            break;
                        case "停站":
                            if (leftTrainType == "通过")
                            {
                                if (thisTrainArr - leftTrainArr <= tongdao)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (leftTrainType == "停站")
                            {
                                if (thisTrainArr - leftTrainArr <= daodao)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                                if (Math.Abs(thisTrainDep - leftTrainDep) <= fafa)
                                {
                                    neibourTrain.Add(leftTrain);
                                }

                            }
                            if (leftTrainType == "终到")
                            {
                                if (thisTrainArr - leftTrainArr <= daodao)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            break;
                    }

                }

                //2.2 再判断左列车
                if (rightTrain != null)
                {
                    int rightTrainArr = Convert.ToInt32(secTimetable.Rows[rightTrainIndex][thisStation + "到"].ToString());
                    int rightTrainDep = Convert.ToInt32(secTimetable.Rows[rightTrainIndex][thisStation + "发"].ToString());
                    string rightTrainType = GetTrainType(thisTrain, thisStation);//左列车作业类型
                    switch (thisTrainType)// 始发 终到 通过 停站
                    {
                        case "始发":
                            if (rightTrainType == "始发")
                            {
                                if (rightTrainDep - thisTrainDep <= fafa)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "通过")
                            {
                                if (rightTrainDep - thisTrainDep <= fatong)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "停站")
                            {
                                if (rightTrainDep - thisTrainDep <= fafa)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            break;

                        case "终到":
                            if (rightTrainType == "终到")
                            {
                                if (rightTrainArr - thisTrainArr <= daodao)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "通过")
                            {
                                if (rightTrainArr - thisTrainArr <= daotong)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "停站")
                            {
                                if (rightTrainArr - thisTrainArr <= daodao)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            break;

                        case "通过":
                            if (rightTrainType == "始发")
                            {
                                if (rightTrainDep - thisTrainDep <= tongfa)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "通过")
                            {
                                if (rightTrainDep - thisTrainDep <= tongtong)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "停站")
                            {
                                if (rightTrainArr - thisTrainDep <= tongdao)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            if (rightTrainType == "终到")
                            {
                                if (rightTrainArr - thisTrainArr <= tongdao)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            break;
                        case "停站":
                            if (rightTrainType == "通过")
                            {
                                if ((rightTrainArr >= thisTrainArr) && (rightTrainArr <= thisTrainDep))
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                                if (rightTrainArr - thisTrainArr <= daotong)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                                if (thisTrainDep - rightTrainDep <= tongfa)
                                {
                                    neibourTrain.Add(leftTrain);
                                }
                            }
                            if (rightTrainType == "停站")
                            {
                                if (rightTrainArr - thisTrainArr <= daodao)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                                if (Math.Abs(thisTrainDep - rightTrainDep) <= fafa)
                                {
                                    neibourTrain.Add(rightTrain);
                                }

                            }
                            if (rightTrainType == "终到")
                            {
                                if (rightTrainArr - thisTrainArr <= daodao)
                                {
                                    neibourTrain.Add(rightTrain);
                                }
                            }
                            break;
                    }

                }


            }


            return neibourTrain;


        }
        //判断列车作业类型的函数
        public string GetTrainType(string thisTrain, string thisStation)
        {
            string trainType;//列车类型 始发 终到 通过 停站
            DataRow[] trainData = this.SecTimeTable.Select(String.Format("车次 = '{0}' and 方向 = '{1}'", thisTrain, this.Direction));
            if (trainData[0]["始发站"].ToString() == thisStation)
            {
                trainType = "始发";
            }
            if (trainData[0]["终到站"].ToString() == thisStation)
            {
                trainType = "终到";
            }
            if (trainData[0][thisStation + "到"].ToString() == trainData[0][thisStation + "发"].ToString())
            {
                trainType = "通过";
            }
            else
            {
                trainType = "停站";
            }
            return trainType;

        }


    }
}
