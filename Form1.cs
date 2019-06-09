using RawPrint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static class Global
        {
            private static string _selectedprinter = "";

            public static string Selectedprinter
            {
                get { return _selectedprinter; }
                set { _selectedprinter = value; }
            }
        }
        public Form1()
        {
            var myprinters = new List<string>();
            InitializeComponent();
            //lister des imprimantes
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                myprinters.Add(printer);
                comboBox1.Items.Add(printer);
                comboBox1.SelectedIndex = comboBox1.FindStringExact(printer);
                //MessageBox.Show(printer);
                Global.Selectedprinter = printer;

            }
        }
        private Timer timer1;
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Interval = 5000; // in miliseconds
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            button1.Text = "Impression...";
            //détecter les fichiers pdf
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.pdf");
            //imprimer les fichiers
            foreach (var item in filePaths)
            {
  
                printMyPdf(item, Path.GetFileName(item));
                File.Delete(item);
                // Sinon pour déplacer le pdf imprimé vers un rep
                //System.IO.Directory.CreateDirectory("Archive");
                //string[] paths = { AppDomain.CurrentDomain.BaseDirectory, "Archive"};
                //string fullPath = Path.Combine(paths);
                //File.Move(item, fullPath+ @"\" + Path.GetFileName(item));

            }
            button1.Text = "Attente...";
        }

        private static void printMyPdf(string pathofmypdf, string filename)
        {

            string PrinterName = Global.Selectedprinter ;

            // Create an instance of the Printer
            IPrinter printer = new Printer();

            // Print the file
            printer.PrintRawFile(PrinterName, pathofmypdf, filename);
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)

        {
            //détecter les fichiers pdf
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.pdf");
            //imprimer les fichiers
            foreach (var item in filePaths)
            {

                printMyPdf(item, Path.GetFileName(item));
                File.Delete(item);

            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {

            //surveiller un répertoire
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = AppDomain.CurrentDomain.BaseDirectory;

            fileSystemWatcher.Created += FileSystemWatcher_Created;

            fileSystemWatcher.Renamed += FileSystemWatcher_Created;

            //fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            fileSystemWatcher.EnableRaisingEvents = true;
            this.Text = "Impression...";
            button1.Text = "Impression...";
            //InitTimer();
            comboBox1.Enabled = false;
            comboBox1.Enabled = false;

        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.Selectedprinter = comboBox1.SelectedItem.ToString();
        }
    }
}
