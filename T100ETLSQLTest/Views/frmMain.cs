using System;
//using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
//using System.Data.OleDb;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using DevExpress.XtraTab;

namespace T100ETLSQLTest.Views
{
    public partial class frmMain : Form
    {
        private string s_folderpath="";

        private BaseEdit inplaceEditor;

        public frmMain()
        {
            InitializeComponent();
        }

        private void  SetConnection()
        {
            Func.server = this.txt_aps_server.Text;
            Func.user = this.txt_aps_user.Text;
            Func.password = this.txt_aps_pwd.Text;
            Func.dbname = this.txt_aps_dbname.Text;

            Func.t100_server = this.txt_t100_db.Text;
            Func.t100_user = this.txt_t100_user.Text;
            Func.t100_password = this.txt_t100_pwd.Text;
            Func.t100_current_user = this.txt_t100_current_user.Text;

            Func.t100_ent = this.txt_CompanyCode_value.Text;
            Func.t100_site = this.txt_OrgCode_value.Text;
            Func.t100_aps = this.txt_APSCode_value.Text;

        }
        private void button2_Click(object sender, EventArgs e)
        {

            if (gridView1.FocusedRowHandle >= 0)
            {
                query(gridView1.FocusedRowHandle);
             }
         }

        private void query(int FocusedRowHandle)
        {
            string table_name;
            string sql_text;

            string filePath = gridView1.GetDataRow(FocusedRowHandle)["資料表路徑"].ToString();
            table_name = gridView1.GetDataRow(FocusedRowHandle)["資料表名稱"].ToString();
            getSQL(filePath, out sql_text);

            SQLReturnType srt;
            if (sql_text.IndexOf("?") >= 0)
                srt = SQLReturnType.cursor;
            else
                srt = SQLReturnType.table;

            sql_text = sql_text.Replace("?", ":ref");


            try
            {
                if (check_query.Checked)
                {
                    SetConnection();
                    Func.OracleDbConn();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = Func.MyOraSqlConn;

                    cmd.CommandText = "alter session set current_schema=" + txt_t100_current_user.Text;
                    cmd.ExecuteNonQuery();

                    if (srt == SQLReturnType.table)
                    {
                        DataTable dt_table = new DataTable();
                        Func.OracleSqlOpen(dt_table, sql_text);
                        createTab(table_name, dt_table);
                    }
                    else
                    {
                        OracleParameter pp = cmd.Parameters.Add("ref", OracleDbType.RefCursor);
                        pp.Direction = ParameterDirection.Output;

                        cmd.CommandText = sql_text;
                        cmd.ExecuteNonQuery();

                        OracleDataReader dr = ((OracleRefCursor)pp.Value).GetDataReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        createTab(table_name, dt);
                    }
                    Clipboard.SetText(sql_text);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
                MessageBox.Show(sql_text);
            }
        }

        private void getSQL(string pfilePath, out string text)
        {

            text = String.Empty;
            if (pfilePath != string.Empty)
            {
                StreamReader streamReader = new StreamReader(pfilePath);
                text = streamReader.ReadToEnd();
                streamReader.Close();

                text = text.Replace(txt_CompanyCode.Text, txt_CompanyCode_value.Text);
                text = text.Replace(txt_OrgCode.Text, txt_OrgCode_value.Text);
                text = text.Replace(txt_APSCode.Text, txt_APSCode_value.Text);
                text = text.Replace(txt_DBAccount.Text, txt_t100_user.Text);
                text = text.Replace(textcus1.Text, textcus_value1.Text);
                text = text.Replace(textcus2.Text, textcus_value2.Text);
                text = text.Replace(txt_version.Text, txt_version_value.Text);
                text = text.Replace(txt_lang.Text, txt_lang_value.Text);
            }
        }
        

        private void createTab(string table_name, DataTable tbl)
        {

            foreach (DataColumn dc in tbl.Columns)
            {
                dc.ColumnName = dc.ColumnName.ToLower();
            }
            
            //create tab
            string key = (tabControl1.TabPages.Count ).ToString();
            tabControl1.TabPages.Add(key +"." +table_name);
            XtraTabPage tp = tabControl1.TabPages[Int32.Parse(key)];
            GridControl gg = new GridControl();
            gg.Parent = tp;
            gg.Dock = DockStyle.Fill;
            GridView gv = new GridView();
            gg.ViewCollection.Add(gv);
            gg.MainView = gv;
            gv.GridControl = gg;
            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsView.ColumnAutoWidth = false;
            gv.ScrollStyle = ScrollStyleFlags.LiveHorzScroll;
            gv.HorzScrollVisibility = ScrollVisibility.Always;
            gg.DataSource = tbl;
            gv.BestFitColumns();
            gg.ContextMenuStrip = contextMenuStrip1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //open path
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                s_folderpath = folderBrowserDialog1.SelectedPath;
                txt_path.Text = s_folderpath;
            }

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            loadIni();
            s_folderpath = txt_path.Text;

            mytips.SetToolTip(this.button2, "每次執行的語法都會複製到剪貼簿");
        }

        private void button4_Click(object sender, EventArgs e)
        {

            comboBoxEdit1.Properties.Items.Clear();
            comboBoxEdit1.Properties.Items.AddRange(Directory.GetDirectories(s_folderpath).Select(d => new DirectoryInfo(d).Name).ToArray<string>());

        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            Func.txt_winmerge_path = txt_winmerge.Text;
            doDoubleClickByRow(sender);
        }

        private static void doDoubleClickByRow(object sender)
        {
            GridView view = (GridView)sender;

            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            GridHitInfo info = view.CalcHitInfo(pt);

            if (info.InRow || info.InRowCell)
            {

                string filePath = view.GetDataRow(info.RowHandle)["資料表路徑"].ToString();
                if (filePath != string.Empty)
                {
                    try
                    {
                        StreamReader streamReader = new StreamReader(filePath);
                        string text = streamReader.ReadToEnd();
                        streamReader.Close();

                        frmSQLEdit fsql_form = new frmSQLEdit(text, filePath, Func.txt_winmerge_path);
                        fsql_form.ShowDialog();
                    }
                    catch (Exception ee)
                    {

                    }
                }

            }
        }

        private void gridView1_ShownEditor(object sender, EventArgs e)
        {
            inplaceEditor = ((GridView)sender).ActiveEditor;

            inplaceEditor.DoubleClick += inplaceEditor_DoubleClick;
        }

        private void gridView1_HiddenEditor(object sender, EventArgs e)
        {
            if (inplaceEditor != null)
            {

                inplaceEditor.DoubleClick -= inplaceEditor_DoubleClick;

                inplaceEditor = null;

            }
        }

        void inplaceEditor_DoubleClick(object sender, EventArgs e)
        {

            BaseEdit editor = (BaseEdit)sender;

            GridControl grid = (GridControl)editor.Parent;

            Point pt = grid.PointToClient(Control.MousePosition);

            GridView view = (GridView)grid.FocusedView;

            Func.txt_winmerge_path = txt_winmerge.Text;
            doDoubleClickByRow(view);

        }


        private void button3_Click(object sender, EventArgs e)
        {
            string table_name;
            string filePath;

            if (gridView1.FocusedRowHandle <0 )
                return;

            filePath = gridView1.GetDataRow(gridView1.FocusedRowHandle)["資料表路徑"].ToString();
            table_name = gridView1.GetDataRow(gridView1.FocusedRowHandle)["資料表名稱"].ToString();

            setStatus("目前處理:" + table_name);
            checkTableSchema(table_name, filePath);
            setStatus("目前處理:" + table_name+" ok");

        }

        private void checkTableSchema( string ptable_name, string pfile_path)
        {
            string sql_text;

            getSQL(pfile_path, out sql_text);

            SQLReturnType srt;
            if (sql_text.IndexOf("?") >= 0)
                srt = SQLReturnType.cursor;
            else
                srt = SQLReturnType.table;

            DataTable t100dt = new DataTable();
            try
            {
                SetConnection();
                if (srt == SQLReturnType.table)
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = Func.MyOraSqlConn;
                    cmd.CommandText = sql_text;
                    OracleDataReader dr = cmd.ExecuteReader();
                    t100dt = dr.GetSchemaTable();
                }
                else
                {
                    sql_text = sql_text.Replace("?", ":ref");
                    OracleCommand  cmd = new OracleCommand();
                    cmd.Connection = Func.MyOraSqlConn;
                    OracleParameter pp = cmd.Parameters.Add("ref", OracleDbType.RefCursor);
                    pp.Direction = ParameterDirection.Output;

                    cmd.CommandText = sql_text;
                    cmd.ExecuteNonQuery();

                    OracleDataReader dr = ((OracleRefCursor)pp.Value).GetDataReader();

                    t100dt = dr.GetSchemaTable();
                }

                string sapssql = "select top 1 ";
                foreach (DataRow r in t100dt.Rows)
                {
                    sapssql = sapssql + r[0].ToString() + ",";
                }
                sapssql = sapssql.ToLower().Substring(0, sapssql.Length - 1) + " from " + ptable_name;
                DataSet apsds = new DataSet();
                Func.SqlOpen(apsds, sapssql);

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void setStatus(string msg)
        {
            toolStripStatusLabel1.Text = msg;
        }

        private void tabControl1_CloseButtonClick(object sender, EventArgs e)
        {
            XtraTabPage t = ((XtraTabControl)sender).SelectedTabPage;
            if (t is XtraTabPage)
            {
                tabControl1.TabPages.Remove(t);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string table_name;
            string filePath;

            SetConnection();
            Func.OracleDbConn();
            Func.DbConn();

            for (int i = 0; i < gridView1.RowCount; i++)
            {
                filePath = gridView1.GetDataRow(i)["資料表路徑"].ToString();
                table_name = gridView1.GetDataRow(i)["資料表名稱"].ToString();
                setStatus("目前處理:"+table_name);
                checkTableSchema(table_name, filePath);
                setStatus("目前處理:" + table_name+" ok");
            }
        }

        private void loadIni()
        {
            if (Properties.Settings.Default.txt_path != string.Empty)
            {
                txt_path.Text = Properties.Settings.Default.txt_path;
                txt_CompanyCode.Text = Properties.Settings.Default.txt_CompanyCode;
                txt_OrgCode.Text = Properties.Settings.Default.txt_OrgCode;
                txt_APSCode.Text = Properties.Settings.Default.txt_APSCode;
                txt_DBAccount.Text = Properties.Settings.Default.txt_DBAccount;
                txt_CompanyCode_value.Text = Properties.Settings.Default.txt_CompanyCode_value;
                txt_OrgCode_value.Text = Properties.Settings.Default.txt_OrgCode_value;
                txt_APSCode_value.Text = Properties.Settings.Default.txt_APSCode_value;
                txt_t100_db.Text = Properties.Settings.Default.txt_t100_db;
                txt_t100_user.Text = Properties.Settings.Default.txt_t100_user;
                txt_t100_pwd.Text = Properties.Settings.Default.txt_t100_pwd;
                txt_t100_current_user.Text = Properties.Settings.Default.txt_t100_current_user;
                txt_aps_server.Text = Properties.Settings.Default.txt_aps_server;
                txt_aps_user.Text = Properties.Settings.Default.txt_aps_user;
                txt_aps_pwd.Text = Properties.Settings.Default.txt_aps_pwd;
                txt_aps_dbname.Text = Properties.Settings.Default.txt_aps_dbname;
                check_query.Checked = (Properties.Settings.Default.check_query == "1" ? true : false);
                txt_winmerge.Text = Properties.Settings.Default.txt_winmerge;
                txt_lang.Text = Properties.Settings.Default.txt_lang;
                txt_lang_value.Text = Properties.Settings.Default.txt_lang_value;
            }
        }


        private void saveIni()
        {
                Properties.Settings.Default.txt_path = txt_path.Text;
                Properties.Settings.Default.txt_CompanyCode = txt_CompanyCode.Text;
                Properties.Settings.Default.txt_OrgCode = txt_OrgCode.Text;
                Properties.Settings.Default.txt_APSCode = txt_APSCode.Text;
                Properties.Settings.Default.txt_DBAccount = txt_DBAccount.Text;
                Properties.Settings.Default.txt_CompanyCode_value = txt_CompanyCode_value.Text;
                Properties.Settings.Default.txt_OrgCode_value = txt_OrgCode_value.Text;
                Properties.Settings.Default.txt_APSCode_value = txt_APSCode_value.Text;
                Properties.Settings.Default.txt_t100_db = txt_t100_db.Text;
                Properties.Settings.Default.txt_t100_user = txt_t100_user.Text;
                Properties.Settings.Default.txt_t100_pwd = txt_t100_pwd.Text;
                Properties.Settings.Default.txt_t100_current_user = txt_t100_current_user.Text;
                Properties.Settings.Default.txt_aps_server = txt_aps_server.Text;
                Properties.Settings.Default.txt_aps_user = txt_aps_user.Text;
                Properties.Settings.Default.txt_aps_pwd = txt_aps_pwd.Text;
                Properties.Settings.Default.txt_aps_dbname = txt_aps_dbname.Text;
                Properties.Settings.Default.check_query = (check_query.Checked == true ? "1" : "0");
                Properties.Settings.Default.txt_winmerge = txt_winmerge.Text;
                Properties.Settings.Default.txt_lang = txt_lang.Text;
                Properties.Settings.Default.txt_lang_value = txt_lang_value.Text;
                Properties.Settings.Default.Save();
                
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Clear();
            for (int i = 0; i <= gridView1.RowCount - 1; i++)
            {
                    query(i);
            }
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] array_path = Directory.GetFiles(s_folderpath +"\\"+ comboBoxEdit1.Properties.Items[comboBoxEdit1.SelectedIndex].ToString()+"\\SQL\\" );
            DataTable T = new DataTable();
            DataColumn col = T.Columns.Add("資料表名稱");
            DataColumn col2 = T.Columns.Add("資料表路徑");
            col.DataType = System.Type.GetType("System.String");
            col2.DataType = System.Type.GetType("System.String");
            for (int j = 0; j < array_path.Length; j++)
            {
                DataRow row = T.NewRow();
                row[0] = Path.GetFileNameWithoutExtension(array_path[j]);
                row[1] = array_path[j];
                T.Rows.Add(row);
            }
            gridControl1.DataSource = T;

            tabControl1.TabPages.Clear();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveIni();
        }

        private void exportXlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridControl gg =(GridControl)contextMenuStrip1.SourceControl;
            if (gg != null)
            {
                if (saveFileDialog1.ShowDialog()== DialogResult.OK)
                {
                    gg.ExportToXls(saveFileDialog1.FileName);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex <= 0)
            {
                MessageBox.Show("請先選擇模組!");
                return;
            }
            string[] array_path = Directory.GetFiles(s_folderpath + "\\" + comboBoxEdit1.Properties.Items[comboBoxEdit1.SelectedIndex].ToString() + "\\SQL\\");
            for (int j = 0; j < array_path.Length; j++)
            {

                byte[] byteData = File.ReadAllBytes(array_path[j]);

                var textDetect = new TextEncodingDetect();
                TextEncodingDetect.Encoding encoding = textDetect.DetectEncoding(byteData, byteData.Length);

                if (encoding == TextEncodingDetect.Encoding.UTF8_BOM ) 
                {
                    string s = array_path[j] + " 是UTF8格式，請轉成無BOM格式!";
                    MessageBox.Show(s);
                    return;
                }
                
            }
            MessageBox.Show("檢查完成，全部都是UTF8無BOM格式");

        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetConnection();
            frmQuery f = new frmQuery();
            f.Show();
        }

    }
}
