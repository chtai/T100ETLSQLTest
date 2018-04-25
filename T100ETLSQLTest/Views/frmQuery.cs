using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTab;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Utils;
using DevExpress.XtraEditors;

namespace T100ETLSQLTest.Views
{
    public partial class frmQuery : Form
    {

        private BaseEdit inplaceEditor;


        public frmQuery()
        {
            InitializeComponent();
        }

        private void btn_query_Click(object sender, EventArgs e)
        {
            string sql = rtbText.Text;
            Func.OracleDbConn();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = Func.MyOraSqlConn;

            cmd.CommandText = "alter session set current_schema=" + Func.t100_current_user;
            cmd.ExecuteNonQuery();

            DataTable dt_table = new DataTable();
            Func.OracleSqlOpen(dt_table, sql);
            createTab(dt_table);
        }


        private void createTab( DataTable tbl)
        {

            foreach (DataColumn dc in tbl.Columns)
            {
                dc.ColumnName = dc.ColumnName.ToLower();
            }

            //create tab
            string key = (tabControl1.TabPages.Count).ToString();
            tabControl1.TabPages.Add(key);
            XtraTabPage tp = tabControl1.TabPages[Int32.Parse(key)];
            tp.ShowCloseButton = DefaultBoolean.True;
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

        private void frmQuery_Load(object sender, EventArgs e)
        {
            string s_path = Application.StartupPath+ "\\query\\";
            string[] array_path = Directory.GetFiles(s_path);
            DataTable T = new DataTable();
            DataColumn col = T.Columns.Add("SQL範本");
            DataColumn col2 = T.Columns.Add("路徑");
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



            Func.OracleDbConn();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = Func.MyOraSqlConn;

            cmd.CommandText = "alter session set current_schema=" + Func.t100_current_user;
            cmd.ExecuteNonQuery();
            string sql="select max(psos002) c from psos_t where 1=1   and psosent="+Func.t100_ent  + " and psossite="+Func.AA(Func.t100_site) + " and psos001="+Func.AA(Func.t100_aps);
            cmd.CommandText = sql;
            txt_max_ver.Text = cmd.ExecuteScalar().ToString();

        }

        private void btn_clear_Click(object sender, EventArgs e)
        {

            tabControl1.TabPages.Clear();
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            doDoubleClickByRow(sender);
        }

        private  void doDoubleClickByRow(object sender)
        {
            GridView view = (GridView)sender;

            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            GridHitInfo info = view.CalcHitInfo(pt);

            if (info.InRow || info.InRowCell)
            {

                string filePath = view.GetDataRow(info.RowHandle)["路徑"].ToString();
                if (filePath != string.Empty)
                {
                    try
                    {
                        StreamReader streamReader = new StreamReader(filePath);
                        string text = streamReader.ReadToEnd();
                        rtbText.Text= text;
                        streamReader.Close();
                 
                    }
                    catch (Exception ee)
                    {

                    }
                }

            }
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

            doDoubleClickByRow(view);

        }

        private void gridView1_ShownEditor(object sender, EventArgs e)
        {
            inplaceEditor = ((GridView)sender).ActiveEditor;

            inplaceEditor.DoubleClick += inplaceEditor_DoubleClick;
        }

        private void tabControl1_CloseButtonClick(object sender, EventArgs e)
        {
            XtraTabPage t = ((XtraTabControl)sender).SelectedTabPage;
            if (t is XtraTabPage)
            {
                tabControl1.TabPages.Remove(t);
            }
        }

        private void exportXlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridControl gg = (GridControl)contextMenuStrip1.SourceControl;
            if (gg != null)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    gg.ExportToXls(saveFileDialog1.FileName);
                }
            }
        }

    }
}
