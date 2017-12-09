using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {

        string substringDirectory;

        public Form1()
        {
            InitializeComponent();

            treeView.Nodes.Clear();
            String path = "K:\\";

            treeView.Nodes.Add(path);
            PopulateTreeView(path, treeView.Nodes[0], 1);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void datagridAddMovie(string filePath, string movietitle)
        {
            string[] row = new string[] { filePath, movietitle, filePath };
            dataGridView1.Rows.Add(row);
            dataGridView1.Refresh();

        }

        private string GetMKVTitle(string filePath)
        {
            bool bTagFound = false;
            
            Debug.WriteLine("GetMKVTitle: " + filePath);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\MediaInfo.exe",
                    Arguments = "\"" + filePath + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Match match = Regex.Match(line, @"Movie name\s+:(.+)$");
                if (match.Success)
                {
                    bTagFound = true;
                    Debug.WriteLine(line);
                    return match.Groups[1].Value.Trim();
                }
            }

            if (!bTagFound)
            {
                Debug.WriteLine("ERROR: COULD NOT GET TITLE: " + filePath);
                updateMKVFile(filePath, "N/A");
                return "N/A";
            }
            else { return null; }
        }

        private void InitializeDataGrid()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            dataGridView1.ColumnCount = 3;
            dataGridView1.Columns[0].Name = "Filename";
            dataGridView1.Columns[1].Name = "Title Property";
            dataGridView1.Columns[2].Name = "FilenameSaved";
            dataGridView1.Columns[2].Visible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void listfiles(string Dpath)
        {
            InitializeDataGrid();

            DirectoryInfo DirInfo = new DirectoryInfo(Dpath);

            foreach (FileInfo fi in DirInfo.GetFiles("*.mkv"))
            {
                string MovieTitle = GetMKVTitle(fi.FullName);
                if (MovieTitle != null)
                {
                    datagridAddMovie(fi.FullName, MovieTitle);
                }

            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void PopulateTreeView(string directoryValue, TreeNode parentNode, int level)
        {
            Debug.WriteLine("PopulateTreeView: " + directoryValue + " " + level);


            string[] directoryArray = Directory.GetDirectories(directoryValue);

            try
            {
                if (directoryArray.Length != 0)
                {
                    foreach (string directory in directoryArray)
                    {
                        substringDirectory = directory.Substring(directory.LastIndexOf('\\') + 1);
                        if (substringDirectory.Contains("System Volume Information") || substringDirectory.Contains("$RECYCLE.BIN"))
                        {
                            Debug.WriteLine("Skipped " + directoryValue);
                        }
                        else
                        { 
                            Debug.WriteLine(substringDirectory);
                            TreeNode myNode = new TreeNode(substringDirectory);
                            parentNode.Nodes.Add(myNode);
                            PopulateTreeView(directory, myNode, level+=1);
                            dataGridView1.Refresh();

                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                parentNode.Nodes.Add("Access denied");
            } // end catch
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // MessageBox.Show(treeView.SelectedNode.FullPath.ToString());
            listfiles(treeView.SelectedNode.FullPath.ToString());
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                updateMKVFile(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
            }
            else if (e.ColumnIndex == 0)
            {
                Debug.WriteLine("{0} renamed to {1}", dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString(), dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                Debug.WriteLine(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                File.Move(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString(), dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                dataGridView1.Rows[e.RowIndex].Cells[2].Value = dataGridView1.Rows[e.RowIndex].Cells[0].Value;

            }
        }

        private void updateMKVFile(string filePath, string newTitle)
        {
            Debug.WriteLine("updateMKVFile " + filePath + " " + newTitle);
            string sArg = "\"" + filePath + "\" --edit info --set \"title=" + newTitle + "\"";
            Debug.WriteLine(sArg);


            if (true)
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "C:\\Program Files (x86)\\MKVtoolnix\\mkvpropedit.exe",
                        //"The Magnificent Seven (2016).mkv" --edit info --set "title=The Magnificent Seven (2016)"
                        Arguments = sArg,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                proc.Start();

                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    Debug.WriteLine(line);
                    
                }


            }


        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                listfiles(treeView.SelectedNode.FullPath.ToString());
            }


        }
    }
}
