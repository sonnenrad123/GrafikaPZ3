using Microsoft.Win32;
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

namespace PZ3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Promenjive za zoom and pan
        private Point start = new Point();
        private Point diffOffset = new Point();
        private int zoomMax = 10;
        private int zoomMaxOut = -5;
        private int zoomCurent = 1;
        bool middleMouseButtonPressed = false;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "xml";
            openFileDialog.Filter = "XML Files|*.xml";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using(new WaitCursor())
                    {
                        string location = openFileDialog.FileName;
                        ImportEntities.LoadAndParseXML(location);

                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Please, provide a valid xml file.", "Invalid file", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }











        #region Transformacije
        private void MainVP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainVP.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translateT.OffsetX;
            diffOffset.Y = translateT.OffsetY;
        }
        private void MainVP_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainVP.ReleaseMouseCapture();
        }

        private void MainVP_MouseMove(object sender, MouseEventArgs e)
        {
            if (MainVP.IsMouseCaptured)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100 ) / w ;
                double translateY = -(offsetY * 100 ) / h ; 

                

                if (middleMouseButtonPressed)//da li rotiramo 
                {
                    double newAngleX = rotateX.Angle + translateY; //kursor gore dole -> rotiramo oko X ose
                    double newAngleY = rotateY.Angle - translateX; //kursor levo desno -> rotiramo oko Y ose
                    rotateX.Angle = newAngleX % 360;
                    rotateY.Angle = newAngleY % 360;
                    start = end;
                }
                else
                {
                    translateT.OffsetX = diffOffset.X + (translateX *2 / (100 * ScaleT.ScaleX));
                    translateT.OffsetY = diffOffset.Y + (translateY *2 / (100 * ScaleT.ScaleX));
                    translateT.OffsetZ = translateT.OffsetZ;
                }
            }
        }

        private void MainVP_MouseWheel(object sender, MouseWheelEventArgs e)
        {


            double scaleX = 1;
            double scaleY = 1;
            double scaleZ = 1;
            if (e.Delta > 0 && zoomCurent < zoomMax) //e.delta predstavlja broj koliko je scroll "okrenut"
            {
                scaleX = ScaleT.ScaleX + 0.1;
                scaleY = ScaleT.ScaleY + 0.1;
                scaleZ = ScaleT.ScaleZ + 0.1;
                ScaleT.CenterX = 5;
                ScaleT.CenterY = 5;
                ScaleT.CenterZ = 0;
                zoomCurent++;
                ScaleT.ScaleX = scaleX;
                ScaleT.ScaleY = scaleY;
                ScaleT.ScaleZ = scaleZ;
            }
            else if (e.Delta <= 0 && zoomCurent > zoomMaxOut)
            {
                scaleX = ScaleT.ScaleX - 0.1;
                scaleY = ScaleT.ScaleY - 0.1;
                scaleZ = ScaleT.ScaleZ - 0.1;
                ScaleT.CenterX = 5;
                ScaleT.CenterY = 5;
                ScaleT.CenterZ = 0;
                zoomCurent--;
                ScaleT.ScaleX = scaleX;
                ScaleT.ScaleY = scaleY;
                ScaleT.ScaleZ = scaleZ;
            }
        }

        private void MainVP_MouseDown(object sender, MouseButtonEventArgs e)//uz pomoc ovoga cemo sami prepoznati kada je skrol pritisnut
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                middleMouseButtonPressed = true;
                MainVP.CaptureMouse();
                start = e.GetPosition(this);
                diffOffset.X = translateT.OffsetX;
                diffOffset.Y = translateT.OffsetY;
            }
        }

        private void MainVP_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (middleMouseButtonPressed)
            {

            }
            if (e.MiddleButton == MouseButtonState.Released)
            {
                middleMouseButtonPressed = false;
                MainVP.ReleaseMouseCapture();
            }
        }

        #endregion


    }
    public class WaitCursor : IDisposable
    {
        private Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }
}
