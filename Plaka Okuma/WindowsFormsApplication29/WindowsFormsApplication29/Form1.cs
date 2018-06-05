using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Tesseract;


using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using MySql.Data.MySqlClient;

namespace WindowsFormsApplication29
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
                var img = new Bitmap(pictureBox2.Image);
                var ocr = new TesseractEngine(@"C:\Users\User\Desktop\anil_bitir\WindowsFormsApplication29\WindowsFormsApplication29\bin\Debug\tessdata", "eng", EngineMode.TesseractAndCube);
                var page = ocr.Process(img);
            
                textBox1.Text = page.GetText();
          
        }
        Bitmap KesilenResim;
        
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox4_Click(sender,e);
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap kaynakResmi, filtreliResim;
            OtsuThreshold otsuFiltre = new OtsuThreshold();
            kaynakResmi = KesilenResim;
            //orjinal resim gösteriliyor
            pictureBox2.Image = kaynakResmi;
         
            //resmi eğer renkliyse önce griye çeviriyor sonra filtre uyguluyor
            //resim zaten griyse direk filtre uyguluyor
            filtreliResim = otsuFiltre.Apply(kaynakResmi.PixelFormat != PixelFormat.Format8bppIndexed ? Grayscale.CommonAlgorithms.BT709.Apply(kaynakResmi) : kaynakResmi);
            //filtre uygulanan resim gösteriliyor
            pictureBox3.Image = filtreliResim;

            //Uygulanan Threshold Değeri form başlığında görünüyor
            this.Text = "Threshold Değeri : " + otsuFiltre.ThresholdValue.ToString();
            button1.Enabled = true;
        }

        int j = 0;
        string[] dizi = new string[600];       
        public void plakaKes(int a)
        {
            CascadeClassifier Classifier = new CascadeClassifier(@"C:\Users\User\Desktop\anil_bitir\car4\cascade\cascade.xml");
            String image_name = dizi[a];
            label1.Text = dizi[a];

            Mat img = CvInvoke.Imread(image_name, Emgu.CV.CvEnum.ImreadModes.AnyColor);
            Image<Bgr, Byte> imgInput = img.ToImage<Bgr, Byte>();
            var imgGray = imgInput.Convert<Gray, byte>();

            Rectangle[] rectangles = Classifier.DetectMultiScale(imgGray, 1.2, 1, new Size(10, 10), new Size(1000, 1000));
            foreach (var rectangle in rectangles)
            {
                CvInvoke.Rectangle(img, rectangle, new MCvScalar(0, 0, 255), 2);
                Image<Bgr, Byte> img_cut = imgInput;
                img_cut.ROI = rectangle;
            }
            System.Drawing.Image pMyImage = imgInput.ToBitmap();   /// Kesilmiş olan plakayı pictureboxa yükledik.
            pictureBox2.Image = pMyImage;

            Image<Bgr, Byte> de = img.ToImage<Bgr, Byte>(); /// Araç resmini pictuereBoxa verdik.
            System.Drawing.Image pMyImage1 = de.ToBitmap();
            pictureBox1.Image = pMyImage1;

            KesilenResim = imgInput.ToBitmap();
            button3.Enabled = true;
        }
        string[] ayir=new string[50];
        string[] ArananPlakalar;
       
        public void Veritabanı()
        {
            try
            {
                string aranan = "";
                int i = 1;
                MySqlConnection mysqlbaglan = new MySqlConnection("Server=localhost;Database=plakatanima;Uid=root;Pwd='';SslMode=none");
                MySqlDataReader dr;
                MySqlCommand cmd;
                char[] ayrac = { ' ', '\n' };
                mysqlbaglan.Open();
                cmd = new MySqlCommand("select * from arananaraclar", mysqlbaglan);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    aranan = dr["Plaka"].ToString() + " " + aranan;
                    ayir = aranan.Split(ayrac);
                }

                string[] bulunan = textBox1.Text.Split(ayrac);
                for (int q = 1; q < bulunan.Length; q += 3)
                {
                    if (ayir[q] == bulunan[i])
                    {
                        if (ayir[q + 1] == bulunan[i + 1])
                        {
                            label2.Visible = true;
                            timer1.Enabled = true;
                            break;
                        }
                    }
                }

                mysqlbaglan.Close();
            }
            catch
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            label2.Visible = false;
        }

        private void pictureBox4_Click(object sender, EventArgs e)  // ileri butonu
        {
            int i = 0;
          
            string dosya_yolu = @"C:\Users\User\Desktop\anil_bitir\WindowsFormsApplication29\WindowsFormsApplication29\arabaa.txt";
            FileStream fs = new FileStream(dosya_yolu, FileMode.Open, FileAccess.Read);

            StreamReader sw = new StreamReader(fs);
            //Okuma işlemi için bir StreamReader nesnesi oluşturduk.
          
            string yazi = sw.ReadLine();
            while (yazi != null)
            {
                if (i == 600)
                {
                    break;
                }
                dizi[i] = yazi.ToString();                
                i++;
                yazi = sw.ReadLine();
            }
            sw.Close();
            fs.Close();

            plakaKes(j);
           
            j++;
            button3_Click(sender, e);
            button1_Click(sender, e);
            Veritabanı();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            j--;
            plakaKes(j);
        }
    }
}

