using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace T100ETLSQLTest.Views
{
    public partial class frmSQLEdit : Form
    {
        private string _sql = "";
        private string _file_path = "";
        private string _winmerge_path = "";

        public frmSQLEdit()
        {
            InitializeComponent();
        }

        public frmSQLEdit(string  p_sql,string p_path,string p_winmerge_path)
        {
            _sql = p_sql;
            _file_path = p_path;
            _winmerge_path = p_winmerge_path;
            InitializeComponent();
        }
   
        private void button1_Click(object sender, EventArgs e)
        {
            string bak_path = Path.GetDirectoryName(_file_path)+ "\\Backup";
            string file_name = Path.GetFileNameWithoutExtension(_file_path);

            if (!Directory.Exists(bak_path))
            {
                Directory.CreateDirectory(bak_path);
            }

            //備份
            System.IO.StreamWriter file = new System.IO.StreamWriter(bak_path + "\\" + file_name + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".txt", false, new UTF8Encoding(false), 100000);
            file.WriteLine(_sql);
            file.Close();

            rtbText.SaveToFile(_file_path, new UTF8Encoding(false));
            MessageBox.Show("save ok");
        }

        private void rtbText_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmSQLEdit_Load(object sender, EventArgs e)
        {
            rtbText.Text = _sql;

            string bak_path = Path.GetDirectoryName(_file_path) + "\\Backup";
            string file_name = Path.GetFileNameWithoutExtension(_file_path);

            if (Directory.Exists(bak_path))
            {
                string[] files = Directory.GetFiles(bak_path);
                foreach (string r in files.Reverse())
                {
                    if (r.Contains(file_name))
                    {
                        comboBox1.Items.Add(Path.GetFileNameWithoutExtension(r));
                    }
                }
                if (comboBox1.Items.Count>0)
                    comboBox1.SelectedIndex = 0;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!File.Exists(_winmerge_path))
            {
                MessageBox.Show("Winmerge路徑錯誤!");
                return;
            }
            string pre_ver_path = Path.GetDirectoryName(_file_path) + @"\Backup\"+comboBox1.Text+".txt";
            string now_ver_path = Path.GetDirectoryName(_file_path) + @"\Backup\目前編輯中.txt" ;
            rtbText.SaveToFile(now_ver_path, new UTF8Encoding(false));
            string parameters = "\"" + pre_ver_path + "\"" + " " + "\"" + now_ver_path + "\"";

            string fullPath = _winmerge_path;//@"C:\Program Files (x86)\WinMerge\WinMergeU.exe";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.GetFileName(fullPath);
            psi.WorkingDirectory = Path.GetDirectoryName(fullPath);
            psi.Arguments = parameters;
            Process.Start(psi);
        }

    }
}
