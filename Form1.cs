using RawPrint;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using System.Net.Http;
using System.Drawing.Printing;
using System.Runtime.Serialization.Json;
using WindowsAutoPrintPdf.DL;
using System.Text;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static class Global
        {
            private static string _selectedprinter = "";
            private static string _urlmobilespool = "";

            public static string Selectedprinter
            {
                get { return _selectedprinter; }
                set { _selectedprinter = value; }
            }
            public static string Urlmobilespool
            {
                get { return _urlmobilespool; }
                set { _urlmobilespool = value; }
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
                textBox1.Text = data["AutoPrintPdf"]["Url Follow GT"];
            }
            //set global url of value of textBox1
            Global.Urlmobilespool = textBox1.Text;
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
        private static string PrintBarCode(string pbc)
        {
            //create barcode img
            string bcodestring = pbc;
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, bcodestring, Color.Black, Color.White, 350, 120);
            //img.Save(bcodestring + ".jpg");

            //print the image
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (senders, args) =>
            {
                args.Graphics.DrawImage(img, 15, 15, img.Width, img.Height);
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                args.Graphics.DrawString(bcodestring, new Font("Arial", 10), Brushes.Black, new PointF(190.0F, 150.0F), sf);
            };
            pd.PrinterSettings.PrinterName = Global.Selectedprinter;
            pd.Print();
            return bcodestring;

        }
        //Method for HTTP GET instanciate just one time
        static readonly HttpClient client = new HttpClient();

        static async Task getHttpBarcode(string url)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await client.GetAsync(Global.Urlmobilespool);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                //MessageBox.Show(responseBody);
                var result = JsonConvert.DeserializeObject<List<CBarcode>>(responseBody);

                //MessageBox.Show(result[0].id.ToString());
                foreach(var element in result)
                {
                    //string barcode = element.barcode;
                    //string str = element.barcode.Replace(";", string.Empty);
                    //MessageBox.Show(str);
                    PrintBarCode(element.barcode.Replace(";", string.Empty));
                    //send get action for delete barcode
                    HttpResponseMessage response2 = await client.GetAsync(Global.Urlmobilespool+"?id="+element.id);
                }

                //https://tst1.follow-gt.fr/print.php?printedBarcodes=1203,1204
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
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

        private Timer TimerMobileSpool;
        public void InitTimer()
        {
            TimerMobileSpool = new Timer();
            TimerMobileSpool.Tick += new EventHandler(timer1_Tick);
            TimerMobileSpool.Interval = 7000; // in miliseconds
            TimerMobileSpool.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //run the func witch get and print barcode
            getHttpBarcode(Global.Urlmobilespool);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("clic sur le bouton ici");
            //get values barcode by get http
            //getHttpBarcode();
            //printBarCode("PBC1234567891234596789QSDF123456789");
            InitTimer();
            button2.Enabled = false;
            textBox1.Enabled = false;
            //Save selected print in config ini file
            if (File.Exists("Configuration.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Configuration.ini");
                data["AutoPrintPdf"]["Default Print"] = comboBox1.SelectedItem.ToString();
                data["AutoPrintPdf"]["Url Follow GT"] = textBox1.Text;
                parser.WriteFile("Configuration.ini", data);
            }
            else
            {
                IniData data = new IniData();
                var parser = new FileIniDataParser();
                data["AutoPrintPdf"]["Default Print"] = comboBox1.SelectedItem.ToString();
                data["AutoPrintPdf"]["Url Follow GT"] = textBox1.Text;
                parser.WriteFile("Configuration.ini", data);
            }
        }

        private void GroupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
