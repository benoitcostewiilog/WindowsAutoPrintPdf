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
using WindowsAutoPrintPdf.DL;
using Newtonsoft.Json;
using ZXing.Common;
using ZXing;
using ZXing.QrCode;

namespace WindowsFormsApp1
{ 
    public partial class Form1 : Form
    {
        static class Global
        {
            private static string _selectedprinter = "";
            private static string _urlmobilespool = "";
            private static int _hauteret = 0;
            private static int _largeuret = 0;
            private static int _taillelabel = 0;

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
            public static int Hauteuet
            {
                get { return _hauteret; }
                set { _hauteret = value; }
            }
            public static int Largeuret
            {
                get { return _largeuret; }
                set { _largeuret = value; }
            }
            public static int taillelabel
            {
                get { return _taillelabel; }
                set { _taillelabel = value; }
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
                textBox2.Text = data["AutoPrintPdf"]["Largeur etiquette"];
                textBox3.Text = data["AutoPrintPdf"]["Hauteur etiquette"];
                textBox4.Text = data["AutoPrintPdf"]["Taille label"];
            }
            //set global url of value of textBox1
            if (textBox3.Text == "") { textBox3.Text = "120";} 
            if ( textBox2.Text == "") { textBox2.Text = "350"; }
            if (textBox4.Text == "") { textBox4.Text = "10"; }
            Global.Urlmobilespool = textBox1.Text;
            Global.Hauteuet = Int32.Parse(textBox3.Text);
            Global.Largeuret = Int32.Parse(textBox2.Text);
            Global.taillelabel = Int32.Parse(textBox4.Text);
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
        private static string PrintBarCode(string pbc, int elarg, int ehaut, int mytaillelabel)
        {
            ////////////// BARCODE 


            string[] barcodetab = pbc.Split(';');
            string emplacementdestination = "";
            if (barcodetab.Length>1)
            {
                emplacementdestination = barcodetab[1];
            }

            QrCodeEncodingOptions options = new QrCodeEncodingOptions();
            options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 500,
                Height = 500,
            };
            var writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;

            var qr = new ZXing.BarcodeWriter();
            qr.Options = options;
            qr.Format = ZXing.BarcodeFormat.QR_CODE;
            Image img = new Bitmap(qr.Write(barcodetab[0]));
   
            ////////////// QR CODE

            //print the image
            PrintDocument pd = new PrintDocument();
            //pd.PrinterSettings.DefaultPageSettings.PaperSize = new PaperSize("210 x 297 mm", Convert.ToInt32(elarg * 0.393701), Convert.ToInt32(ehaut * 0.393701));

            pd.PrintPage += (senders, args) =>
            {
                //Ajout du logo : 
                Image logosociete = Image.FromFile("logo.png");
                int tailleqrcode = Convert.ToInt32(ehaut * (float)0.8);
                args.Graphics.DrawImage(logosociete, 10, Convert.ToInt32(ehaut * (float)0.2), 70,30);

                args.Graphics.DrawImage(img, (elarg - tailleqrcode) / 2, Convert.ToInt32(ehaut*(float)0.2), tailleqrcode, tailleqrcode);
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                //args.Graphics.DrawString(bcodestring, new Font("Arial", 10), Brushes.Black, new PointF((elarg+30)/2, ehaut+30), sf);
                args.Graphics.DrawString(barcodetab[0], new Font("Arial", mytaillelabel), Brushes.Black, new Rectangle(0, ehaut+30, elarg, 0), sf);
                args.Graphics.DrawString(emplacementdestination, new Font("Arial", mytaillelabel), Brushes.Black, new Rectangle(0, ehaut + 45, elarg, 0), sf);

            };
            pd.PrinterSettings.PrinterName = Global.Selectedprinter;
            pd.Print();
            return barcodetab[0];

        }
        //Method for HTTP GET instanciate just one time
        static readonly HttpClient client = new HttpClient();

        static async Task getHttpBarcode(string url)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                //MessageBox.Show("test1");
                HttpResponseMessage response = await client.GetAsync(Global.Urlmobilespool);

                response.EnsureSuccessStatusCode();
      
                string responseBody = await response.Content.ReadAsStringAsync();
   
                var result = JsonConvert.DeserializeObject<List<CBarcode>>(responseBody);

                //MessageBox.Show(result[0].id.ToString());
                foreach(var element in result)
                {
       
                    //PrintBarCode(element.barcode.Replace(";", string.Empty), Global.Largeuret,Global.Hauteuet,10);
                    PrintBarCode(element.barcode, Global.Largeuret, Global.Hauteuet, Global.taillelabel);

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
            toolStripStatusLabel1.Text = "impression Auto Print Pdf en cours...";
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
            TimerMobileSpool.Interval = 4000; // in miliseconds
            TimerMobileSpool.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //run the func witch get and print barcode

            getHttpBarcode(Global.Urlmobilespool);


        }

        private void Button2_Click(object sender, EventArgs e)
        {


            Global.Urlmobilespool = textBox1.Text;
            //Run if Url not empty
            if (Global.Urlmobilespool != "") 
            {
                toolStripStatusLabel1.Text = "Impression MobileSpool en cours...";
                InitTimer();
                button1.Enabled = false;
                comboBox1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                
            }
            else
            {
                MessageBox.Show("Url d'instance vide, veuillez saisir ce parametre");
            }



            //Save selected print in config ini file
            if (File.Exists("Configuration.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Configuration.ini");
                data["AutoPrintPdf"]["Default Print"] = comboBox1.SelectedItem.ToString();
                data["AutoPrintPdf"]["Url Follow GT"] = textBox1.Text;
                data["AutoPrintPdf"]["Largeur etiquette"] = textBox2.Text;
                data["AutoPrintPdf"]["Hauteur etiquette"] = textBox3.Text;
                data["AutoPrintPdf"]["Taille label"] = textBox4.Text;
                textBox4.Text = data["AutoPrintPdf"]["Taille label"];
                parser.WriteFile("Configuration.ini", data);
                Global.Urlmobilespool = textBox1.Text;
            }
            else
            {
                IniData data = new IniData();
                var parser = new FileIniDataParser();
                data["AutoPrintPdf"]["Default Print"] = comboBox1.SelectedItem.ToString();
                data["AutoPrintPdf"]["Url Follow GT"] = textBox1.Text;
                data["AutoPrintPdf"]["Largeur etiquette"] = textBox2.Text;
                data["AutoPrintPdf"]["Hauteur etiquette"] = textBox3.Text;
                data["AutoPrintPdf"]["Taille label"] = textBox4.Text;
                parser.WriteFile("Configuration.ini", data);
                Global.Urlmobilespool = textBox1.Text;
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            int elarg = Int32.Parse(textBox2.Text);
            int ehaut = Int32.Parse(textBox3.Text);
            int tailllab = Int32.Parse(textBox4.Text);
            PrintBarCode("BARCODEDETEST123456789AZERTYUIOP", elarg, ehaut,Global.taillelabel);
            //MessageBox.Show("test");
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

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
