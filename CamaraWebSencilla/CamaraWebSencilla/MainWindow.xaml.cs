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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            miKinect = KinectSensor.KinectSensors.FirstOrDefault();
            miKinect.Start();
            miKinect.ColorStream.Enable();
            miKinect.DepthStream.Enable();
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
                int blanco = 230;
                while (i< largoDatosColor && !encontro && (i/2)<largoDatosColor)
                {
                    if(datosColor[i] >= blanco && datosColor[i+1] >= blanco && datosColor[i+2] >= blanco)
                    {
                        int x = (i / 4) / 640;
                        int y = (i / 4 ) % 640;
                        int z = datosDistancia[i / 4] >> 3;
                        Console.WriteLine("BLANCO x: "+ x + ", y: " + y + ", z: " + z);
                        encontro = true;
                    }
                    i += 4;
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
    }
}