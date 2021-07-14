using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class StationInformation : Form
    {
        public StationInformation()
        {
            InitializeComponent();
        }

        private void StationInformation_Load(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Maximized;

            //车站数据加载
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string filePath = path + "\\Resources\\京沪高铁沿线站点.xlsx";
            //String filePath = Application.StartupPath + "/Resources/京沪高铁沿线站点.xlsx";
            //根据路径打开一个Excel文件并将数据填充到DataSet中
            string strConn = "provider=Microsoft.ACE.OLEDB.12.0; Data Source='" + filePath + "'" + "; Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";//HDR=YES 有两个值:YES/NO,表示第一行是否字段名,默认是YES,第一行是字段名

            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            DataTable da = null;
            strExcel = "select  * from   [sheet1$]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            da = ds.Tables[0];
            stationDataGridView.DataSource = da;
        }
    }
}
