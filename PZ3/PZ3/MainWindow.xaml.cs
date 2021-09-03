using Microsoft.Win32;
using PZ3.Models;
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
using System.Windows.Media.Media3D;
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

        //filter promenjive
        bool ResistZeroOne = true;
        bool ResistOneTwo = true;
        bool ResistGTTwo = true;
        bool ConnectionsZeroThree = true;
        bool ConnectionsThreeFive = true;
        bool ConnectionGTFive = true;
        bool OnlyActiveLines = false;

        //promenjive za hittest
        ToolTip mainToolTip = new ToolTip();

        List<GeometryModel3D> allModels = new List<GeometryModel3D>();

        public MainWindow()
        {
            mainToolTip.IsOpen = false;
            mainToolTip.StaysOpen = false;
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
                    using (new WaitCursor())
                    {
                        string location = openFileDialog.FileName;
                        ImportEntities.LoadAndParseXML(location);
                        ImportEntities.ScaleLatLon();

                        ModelCreator.CreateModels(modelGroup, ImportEntities.substations, ImportEntities.switches, ImportEntities.lines, ImportEntities.nodes);

                        MainVP.MouseLeftButtonDown += HitTestMouseButtonDown;
                        
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
                double translateX = (offsetX * 100) / w;
                double translateY = -(offsetY * 100) / h;



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
                    translateT.OffsetX = diffOffset.X + (translateX * 2 / (100 * ScaleT.ScaleX));
                    translateT.OffsetY = diffOffset.Y + (translateY * 2 / (100 * ScaleT.ScaleX));
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



        #region HitTest




        private void HitTestMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainToolTip.IsOpen = false;
            Point mouseposition = e.GetPosition(MainVP);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);
            PointHitTestParameters pointparams =
                     new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams =
                     new RayHitTestParameters(testpoint3D, testdirection);
            VisualTreeHelper.HitTest(MainVP, null, HTResult, pointparams);
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;


            if (rayResult != null)
            {
                var resultValue = rayResult.ModelHit.GetValue(FrameworkElement.TagProperty);
                if (resultValue is LineEntity)
                {
                    LineEntity line = (LineEntity)resultValue;
                    //vrati sve na default i oboji start end
                    foreach (GeometryModel3D model in modelGroup.Children)
                    {
                        if(model.GetValue(FrameworkElement.TagProperty) is PowerEntity)
                        {
                            PowerEntity modelValue = (PowerEntity)model.GetValue(FrameworkElement.TagProperty);
                            model.Material = new DiffuseMaterial(modelValue.PowerEntityShape.Fill);
                            if (modelValue.Id == line.FirstEnd || modelValue.Id == line.SecondEnd)
                            {
                                model.Material = new DiffuseMaterial(Brushes.YellowGreen);
                            }
                        }

                        
                    }




                }
                if (resultValue is PowerEntity)
                {
                    mainToolTip.Content = ((PowerEntity)resultValue).printToolTip();
                    mainToolTip.IsOpen = true;
                }

            }
            return HitTestResultBehavior.Stop;
        }

        #endregion



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

        private void ZeroOneCB_Checked(object sender, RoutedEventArgs e)
        {
            ResistZeroOne = true;
            DoFilterLines();
        }

        private void ZeroOneCB_Unchecked(object sender, RoutedEventArgs e)
        {
            ResistZeroOne = false;
            DoFilterLines();
        }

        private void OneTwoCB_Checked(object sender, RoutedEventArgs e)
        {
            ResistOneTwo = true;
            DoFilterLines();
        }

        private void OneTwoCB_Unchecked(object sender, RoutedEventArgs e)
        {
            ResistOneTwo = false;
            DoFilterLines();
        }

        private void GtThreeCB_Checked(object sender, RoutedEventArgs e)
        {
            ResistGTTwo = true;
            DoFilterLines();
        }

        private void GtThreeCB_Unchecked(object sender, RoutedEventArgs e)
        {
            ResistGTTwo = false;
            DoFilterLines();
        }

        private void ZeroThreeCB_Checked(object sender, RoutedEventArgs e)
        {
            ConnectionsZeroThree = true;
            DoFilterNodes();
         
        }

        private void ThreeFiveCB_Checked(object sender, RoutedEventArgs e)
        {
            ConnectionsThreeFive = true;
            DoFilterNodes();
        }

        private void GTFiveCB_Checked(object sender, RoutedEventArgs e)
        {
            ConnectionGTFive = true;
            DoFilterNodes();
        }

        private void ActiveLinesCB_Checked(object sender, RoutedEventArgs e)
        {
            OnlyActiveLines = true;
            DoFilterLines();
        }

        private void ZeroThreeCB_Unchecked(object sender, RoutedEventArgs e)
        {
            ConnectionsZeroThree = false;
            DoFilterNodes();
        }

        private void ThreeFiveCB_Unchecked(object sender, RoutedEventArgs e)
        {
            ConnectionsThreeFive = false;
            DoFilterNodes();
        }

        private void GTFiveCB_Unchecked(object sender, RoutedEventArgs e)
        {
            ConnectionGTFive = false;
            DoFilterNodes();
        }

        private void ActiveLinesCB_Unchecked(object sender, RoutedEventArgs e)
        {
            OnlyActiveLines = false;
            DoFilterLines();
        }

        private void DoFilterLines()
        {
            //prikazemo sve onda izbacimo sta treba
            foreach (GeometryModel3D model in modelGroup.Children)
            {
                if (model.GetValue(FrameworkElement.TagProperty) is LineEntity)
                {
                    LineEntity modelValue = (LineEntity)model.GetValue(FrameworkElement.TagProperty);

                    if (modelValue.ConductorMaterial == "Acsr")
                    {
                        model.Material = new DiffuseMaterial(Brushes.HotPink);
                    }
                    else if (modelValue.ConductorMaterial == "Steel")
                    {
                        model.Material = new DiffuseMaterial(Brushes.Aqua);
                    }
                    else
                    {
                        model.Material = new DiffuseMaterial(Brushes.DarkGoldenrod);
                    }
                }


            }
            //samo aktivne linije?
            if (OnlyActiveLines == true)
            {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity)
                    {
                        LineEntity modelValue = (LineEntity)model.GetValue(FrameworkElement.TagProperty);

                        if (modelValue.Active == false)
                        {
                            model.Material = new DiffuseMaterial(Brushes.Transparent);
                        }
                    }


                }
            }
            //iskljucimo nezeljene otpornosti
            if (ResistZeroOne == false)
            {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity)
                    {
                        LineEntity modelValue = (LineEntity)model.GetValue(FrameworkElement.TagProperty);

                        if (modelValue.R >= 0 && modelValue.R <= 1)
                        {
                            model.Material = new DiffuseMaterial(Brushes.Transparent);
                        }
                    }


                }
            }
            if(ResistOneTwo == false)
            {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity)
                    {
                        LineEntity modelValue = (LineEntity)model.GetValue(FrameworkElement.TagProperty);

                        if (modelValue.R > 1 && modelValue.R <= 2)
                        {
                            model.Material = new DiffuseMaterial(Brushes.Transparent);
                        }
                    }


                }
            }
            if(ResistGTTwo == false)
            {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity)
                    {
                        LineEntity modelValue = (LineEntity)model.GetValue(FrameworkElement.TagProperty);

                        if (modelValue.R > 2)
                        {
                            model.Material = new DiffuseMaterial(Brushes.Transparent);
                        }
                    }


                }
            }

        }

        private void DoFilterNodes()
        {
            //prikazemo sve onda izbacimo sta treba
            foreach (GeometryModel3D model in modelGroup.Children)
            {
                if (model.GetValue(FrameworkElement.TagProperty) is  SubstationEntity)
                {
                    model.Material = new DiffuseMaterial(Brushes.Green);
                }
                else if(model.GetValue(FrameworkElement.TagProperty) is SwitchEntity)
                {

                    model.Material = new DiffuseMaterial(Brushes.Red);
                }
                else if(model.GetValue(FrameworkElement.TagProperty) is NodeEntity)
                {
                    model.Material = new DiffuseMaterial(Brushes.Blue);
                }

            }

            if(ConnectionsZeroThree == false)
            {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity) //iskuliramo
                    {
                        
                    }
                    else
                    {
                        if(model.GetValue(FrameworkElement.TagProperty) != null)
                        {
                            PowerEntity modelValue = (PowerEntity)model.GetValue(FrameworkElement.TagProperty);
                            if (modelValue.NumberOfConnections >= 0 && modelValue.NumberOfConnections < 3)
                            {
                                model.Material = new DiffuseMaterial(Brushes.Transparent);
                            }
                        }
                        
                    }


                }
            }
            if(ConnectionsThreeFive == false)
            {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity) //iskuliramo
                    {

                    }
                    else
                    {
                        if (model.GetValue(FrameworkElement.TagProperty) != null)
                        {
                            PowerEntity modelValue = (PowerEntity)model.GetValue(FrameworkElement.TagProperty);
                            if (modelValue.NumberOfConnections >= 3 && modelValue.NumberOfConnections <= 5)
                            {
                                model.Material = new DiffuseMaterial(Brushes.Transparent);
                            }
                        }
                    }
                }
            }
            if (ConnectionGTFive == false) {
                foreach (GeometryModel3D model in modelGroup.Children)
                {
                    if (model.GetValue(FrameworkElement.TagProperty) is LineEntity) //iskuliramo
                    {

                    }
                    else
                    {
                        if (model.GetValue(FrameworkElement.TagProperty) != null)
                        {
                            PowerEntity modelValue = (PowerEntity)model.GetValue(FrameworkElement.TagProperty);
                            if (modelValue.NumberOfConnections > 5)
                            {
                                model.Material = new DiffuseMaterial(Brushes.Transparent);
                            }
                        }
                    }
                }
            }

        }
    }
}
