using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace CamaraWebSencilla
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        KinectSensor miKinect;
        short[] datosDistancia = null;
        public int[][] mapaProfundidad = new int[480][];
        public string[][] mapaBlanco = new string[480][];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            miKinect = KinectSensor.KinectSensors.FirstOrDefault();
            miKinect.Start();
            miKinect.DepthStream.Enable();
            miKinect.ColorStream.Enable();
            miKinect.DepthFrameReady += MiKinect_DepthFrameReady;
            miKinect.ColorFrameReady += miKinect_ColorFrameReady;
            
        }

        private void MiKinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame framesDistancia = e.OpenDepthImageFrame())
            {
                if (framesDistancia == null)
                {
                    Console.WriteLine("Return");
                    return;
                }
                if (datosDistancia == null)
                    datosDistancia = new short[framesDistancia.PixelDataLength];
                framesDistancia.CopyPixelDataTo(datosDistancia);
            }
        }
        void miKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frameImagen = e.OpenColorImageFrame())
            {

                if (frameImagen == null)
                    return;

                byte[] datosColor = new byte[frameImagen.PixelDataLength];

                frameImagen.CopyPixelDataTo(datosColor);
                int largoDatosColor = datosColor.Length;
                int i = 0;
                bool encontro = false;
                int blanco = 254;
                int k = 0, j = 0;
                mapaProfundidad[j] = new int[640];
                mapaBlanco[j] = new string[640];
                i = 0;
                while (i< largoDatosColor)
                {
                    mapaBlanco[j][k] = ""; //j=480 x k=640
                    if (datosColor[i] >= blanco && datosColor[i+1] >= blanco && datosColor[i+2] >= blanco)
                    {
                        int x = (i / 4) / 640;
                        int y = (i / 4 ) % 640;
                        int z = 0;
                        try
                        {
                             z = datosDistancia[(i / 4)+25] >> 3;
                        }
                        catch {
                            z = -9;
                        }
                        
                        mapaBlanco[j][k] = "B"; //j=480 x k=640
                        if(encontro == false)
                        {
                            Console.WriteLine("BLANCO x: " + x + ", y: " + y + ", z: " + z);
                        }
                        encontro = true;
                    }
                    k++;
                    if (((i/4) + 1) % 640 == 0)
                    {
                        j++;
                        if (j != 480)
                        {
                            mapaBlanco[j] = new string[640];
                        }
                        k = 0;
                    }
                    if (j % 480 == 0)
                    {
                        j = 0;
                    }
                    //Fin Calculo de j,k
                    i += 4;
                }
                j = 0;
                k = 0;
                i = 0;
                while (i < (largoDatosColor / 4))
                {
                    //Calculo de j,k
                    int valorDistancia = 0;
                    try
                    {
                        valorDistancia = datosDistancia[i] >> 3;
                    }
                    catch
                    {
                        valorDistancia = -9;
                    }
                    mapaProfundidad[j][k] = valorDistancia; //j=480 x k=640
                    k++;
                    if ((i + 1) % 640 == 0)
                    {
                        j++;
                        if (j != 480)
                        {
                            mapaProfundidad[j] = new int[640];
                        }
                        k = 0;
                    }
                    if (j % 480 == 0)
                    {
                        j = 0;
                    }
                    //Fin Calculo de j,k
                    i++;
                }

                mostrarVideo.Source = BitmapSource.Create(
                    frameImagen.Width, frameImagen.Height,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null,
                    datosColor,
                    frameImagen.Width * frameImagen.BytesPerPixel
                    );
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int j = 0;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Mapa.txt", false))
            {
                while (j < 480)
                {
                    string linea = "";
                    for (int i = 0; i < 640; i++)
                    {
                        if (mapaBlanco[j][i] == "B")
                        {

                            linea += "B " + Math.Abs(mapaProfundidad[j][i]).ToString("0000") + "  ";
                        }
                        else
                        {
                            linea += "X " + Math.Abs(mapaProfundidad[j][i]).ToString("0000") + "  ";
                        }


                    }
                    file.WriteLine(linea);
                    j++;
                }
            }
        }
    }
}