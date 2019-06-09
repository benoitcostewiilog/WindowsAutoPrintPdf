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
using IniParser;
using IniParser.Model;

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
            //list all printer
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                myprinters.Add(printer);
                comboBox1.Items.Add(printer);
                comboBox1.SelectedIndex = comboBox1.FindStringExact(printer);
                //MessageBox.Show(printer);
                Global.Selectedprinter = printer;

            }
            //Get the default printer from config ini file
            if (File.Exists("Configuration.ini")){
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Configuration.ini");
                comboBox1.SelectedIndex = comboBox1.FindStringExact(data["AutoPrintPdf"]["Default Print"]);
            }

        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            button1.Text = "Impression...";
            //Detect pdf File
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.pdf");
            //print the pdf file
            foreach (var item in filePaths)
            {
                PrintMyPdf(item, Path.GetFileName(item));
                File.Delete(item);
            }
            button1.Text = "Attente...";
           
        }

        private static void PrintMyPdf(string pathofmypdf, string filename)
        {

            string PrinterName = Global.Selectedprinter ;
            // Create an instance of the Printer
            IPrinter printer = new Printer();
            // Print the file
            printer.PrintRawFile(PrinterName, pathofmypdf, filename);
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)

        {
            //list all pdf in directory
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.pdf");
            //print pdf file
            foreach (var item in filePaths)
            {

                PrintMyPdf(item, Path.GetFileName(item));
                File.Delete(item);

            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {

            //Watch directory
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = AppDomain.CurrentDomain.BaseDirectory;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Renamed += FileSystemWatcher_Created;
            //fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            fileSystemWatcher.EnableRaisingEvents = true;
            button1.Text = "Impression...";
            //Disable UI controls
            comboBox1.Enabled = false;
            comboBox1.Enabled = false;
            button1.Enabled = false;

            //Save selected print in config ini file
            if (File.Exists("Configuration.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Configuration.ini");
                data["AutoPrintPdf"]["Default Print"] = comboBox1.SelectedItem.ToString();
                parser.WriteFile("Configuration.ini", data);
            }
            else
            {
                IniData data = new IniData();
                var parser = new FileIniDataParser();
                data["AutoPrintPdf"]["Default Print"] = comboBox1.SelectedItem.ToString();
                parser.WriteFile("Configuration.ini", data);
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.Selectedprinter = comboBox1.SelectedItem.ToString();
        }
    }
}
