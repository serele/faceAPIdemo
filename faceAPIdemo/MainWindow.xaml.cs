using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace faceAPIdemo
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IFaceServiceClient _faceServiceClient = new FaceServiceClient("8ad684ea39274825abadcce884a27d42", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var openFrame = new Microsoft.Win32.OpenFileDialog { Filter = "JPEG Image(*.jpg)|*.jpg" };
            var result = openFrame.ShowDialog(this);

            if (!(bool)result)
                return;

            var filePath = openFrame.FileName;
            var fileUri = new Uri(filePath);
            var bitMapSource = new BitmapImage();
            
            bitMapSource.BeginInit();
            bitMapSource.CacheOption = BitmapCacheOption.None;
            bitMapSource.UriSource = fileUri;
            bitMapSource.EndInit();

            faceImage.Source = bitMapSource;

            Title = "Detecting...";
            FaceRectangle[] facesFound = await DetectTheFaces(filePath);
            Title = $"Found {facesFound.Length} faces";

            if (facesFound.Length <= 0) return;

            var drwVisual = new DrawingVisual();
            var drwContext = drwVisual.RenderOpen();
            drwContext.DrawImage(bitMapSource, new Rect(0, 0, bitMapSource.Width, bitMapSource.Height));
            var dpi = bitMapSource.DpiX;
            var resizeFactor = 96 / dpi;

            foreach (var faceRect in facesFound)
            {
                drwContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue, 6),
                    new Rect(faceRect.Left * resizeFactor, faceRect.Top * resizeFactor, faceRect.Width * resizeFactor,
                    faceRect.Height * resizeFactor));
            }
            drwContext.Close();
            var renderToImageCtrl = new RenderTargetBitmap((int)(bitMapSource.PixelWidth * resizeFactor), (int)(bitMapSource.PixelHeight * resizeFactor), 96, 96, PixelFormats.Pbgra32);
            renderToImageCtrl.Render(drwVisual);
            faceImage.Source = renderToImageCtrl;
        }

        private async Task<FaceRectangle[]> DetectTheFaces(string filePath)
        {
            try
            {
                using (var imgStream = File.OpenRead(filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imgStream);
                    var faceRectangles = faces.Select(face => face.FaceRectangle);
                    return faceRectangles.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
