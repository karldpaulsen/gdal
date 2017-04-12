using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Media.Imaging;

namespace PanAndZoom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        geoRaster raster;
        private Point origin;
        private Point start;

        string file = "G:/Sectionals/SaltLakeCitySEC94.tif";

        public MainWindow()
        {
            InitializeComponent();
            raster = new geoRaster(file);

            TransformGroup group = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            canvas.RenderTransform = group;
            canvas.RenderTransformOrigin = new Point(0.0, 0.0);

            this.MouseMove += MainWindow_MouseMove;
            this.MouseWheel += MainWindow_MouseWheel;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.MouseRightButtonDown += MainWindow_MouseRightButtonDown;

            BitmapImage image = new BitmapImage(new Uri(file));

            //content.Source = image;
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

         private void Zoom( double delta, Point dp, UIElement element)
        {
            var st = GetScaleTransform(element);
            var tt = GetTranslateTransform(element);


            double zoom = delta > 0 ? .2 : -.2;
            if (!(delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                return;

            // X,Y offset relative to the scaled display
            Point mp = new Point();
            mp.X = dp.X * st.ScaleX + tt.X;
            mp.Y = dp.Y * st.ScaleY + tt.Y;

            // set the zoom level
            st.ScaleX += zoom;
            st.ScaleY += zoom;

            // Calcluate the offset
            tt.X = mp.X - dp.X * st.ScaleX;
            tt.Y = mp.Y - dp.Y * st.ScaleY;

            this.Scale.Text = st.ScaleX.ToString("F1") + "," + st.ScaleY.ToString("F1");
            this.TranslateXY.Text = tt.X.ToString("F0") + "," + tt.Y.ToString("F0");
        }

        private void ZoomOutHandler(object sender, RoutedEventArgs e)
        {
            // Zoom from center of display
            Point dp = new Point();
            dp.X = canvas.ActualWidth / 2;
            dp.Y = canvas.ActualHeight / 2;

            Zoom(-1.0, dp, canvas);
        }

        private void ZoomInHandler(object sender, RoutedEventArgs e)
        {
             // Zoom from center of display
            Point dp = new Point();
            dp.X = canvas.ActualWidth / 2;
            dp.Y = canvas.ActualHeight / 2;

            Zoom(1.0, dp, canvas);
        }

        private void ZoomResetHandler(object sender, RoutedEventArgs e)
        {
            var st = GetScaleTransform(canvas);
            st.ScaleX = 1.0;
            st.ScaleY = 1.0;

            var tt = GetTranslateTransform(canvas);
            tt.X = 0.0;
            tt.Y = 0.0;

            this.Scale.Text = st.ScaleX.ToString("F1") + "," + st.ScaleY.ToString("F1");
            this.TranslateXY.Text = tt.X.ToString("F0") + "," + tt.Y.ToString("F0");
        }


        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Use border as orgin so there won't
            // be jitter if the mouse is moving
            Point dp = new Point();
            dp.X = e.GetPosition(border).X;
            dp.Y = e.GetPosition(border).Y;

            Zoom(e.Delta, dp, canvas);
        }


        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var st = GetScaleTransform(canvas);
            var tt = GetTranslateTransform(canvas);

            //BitmapImage image = (BitmapImage)content.Source;
            
            // Current position relative to border origin.
            // Use border to prevent jitter when panning
            Point p = e.GetPosition(border);

            if (p.X >= 0 && p.Y >= 0)
            {
                this.DisplayXY.Text = p.X.ToString("F0") + "," + p.Y.ToString("F0");

                if (canvas.IsMouseCaptured)
                {
                    Vector v = start - p;
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                    this.TranslateXY.Text = tt.ToString();
                }

                // Transform display x,y to map x,y
                Point mp = new Point((p.X - tt.X) * (raster.width / (canvas.ActualWidth * st.ScaleX)), 
                        (p.Y - tt.Y) * (raster.height / (canvas.ActualWidth * st.ScaleY)));
                this.MapXY.Text = mp.X.ToString("F0") + "," + mp.Y.ToString("F0");


                Point c = raster.px2coord(mp);
                this.srsCoord.Text = c.X.ToString("F4") + "," + c.Y.ToString("F4");

                this.Scale.Text = st.ScaleX.ToString("F1") + "," + st.ScaleY.ToString("F1");
                this.TranslateXY.Text = tt.X.ToString("F0") + "," + tt.Y.ToString("F0");

                this.LatLon.Text = raster.coord2LatLon(c).ToString();
            }
        }

        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var st = GetScaleTransform(canvas);
            var tt = GetTranslateTransform(canvas);

            Point p = e.GetPosition(border);

            // Transform display pixel coordinate to map coordinates
            Point mp = new Point((p.X - tt.X) * (raster.width / (canvas.ActualWidth * st.ScaleX)),
                    (p.Y - tt.Y) * (raster.height / (canvas.ActualWidth * st.ScaleY)));

            Point c = raster.px2coord(mp);

            GeoCoordinate ll = raster.coord2LatLon(c);

             MessageBox.Show(
                "Display: " + p.X.ToString("F0") + "," + p.Y.ToString("F0") + Environment.NewLine +
                "Coord: " + c.X.ToString("F4") + "," + c.Y.ToString("F4") + Environment.NewLine +
                "Map: " + mp.X.ToString("F0") + "," + mp.Y.ToString("F0") + Environment.NewLine +
                "Lat/Lon: " + ll.ToString()
           );
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tt = GetTranslateTransform(canvas);

            // Use the position of the border because
            // it is fixed and therefore won't cause jitter
            // when moving the mouse.
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);
            canvas.Cursor = Cursors.Hand;
            canvas.CaptureMouse();
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas.ReleaseMouseCapture();
            canvas.Cursor = Cursors.Arrow;
        }
    }
}
