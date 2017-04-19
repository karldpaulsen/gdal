using System;
using OSGeo.GDAL;
using OSGeo.OSR;
using System.Device.Location;
using System.Windows;


namespace PanAndZoom
{
    class geoRaster
    {
        Dataset ds;

        double[] GeoTransform = new double[6];

        // Public Properties
        public double height;
        public double width;
        public Point origin;
        public double pixelSizeX;
        public double pixelSizeY;

        public int xOffset = 0;
        public int yOffset = 0;

        private double xScale { get; set; }
        private double yScale { get; set; }


        CoordinateTransformation toLatLon;
        CoordinateTransformation toCoord;

        SpatialReference srsLCC = new SpatialReference("");
        SpatialReference srsLatLon = new SpatialReference("");

        public geoRaster(string file)
        {
            Gdal.AllRegister();

            ds = Gdal.Open(file, Access.GA_ReadOnly);
            
            ds.GetGeoTransform(GeoTransform);

            height = ds.RasterYSize;
            width = ds.RasterXSize;

            origin = new Point(GeoTransform[1], GeoTransform[3]);

            pixelSizeX = GeoTransform[1];
            pixelSizeY = GeoTransform[5];

            string s = ds.GetProjection();

            srsLCC = new SpatialReference(s);
            srsLatLon.SetWellKnownGeogCS("WGS84");

            toLatLon = new CoordinateTransformation(srsLCC, srsLatLon);
            toCoord =  new CoordinateTransformation(srsLatLon, srsLCC);

            xScale = 1.0;
            yScale = 1.0;
        }


        // Does a transformation between display points
        // and east,north point in the rasters spatial
        // system

        public Point px2coord(Point mp)
        {
            Point v = new Point();
            v.X = GeoTransform[0] + (mp.X * GeoTransform[1]);
            v.Y = GeoTransform[3] + (mp.Y * GeoTransform[5]);

            return v;
        }

 
        // Does a transformation from spatial reference system
        // to raster x,y

        public Point coord2px(Point p)
        {
            Point v = new Point();
            v.X = ((p.X - GeoTransform[0]) / GeoTransform[1]);
            v.Y = ((p.Y - GeoTransform[3]) / GeoTransform[5]);
            return v;
        }

        // Does a transformation from spatial reference system
        // to Lat/Lon
        
        public GeoCoordinate coord2LatLon(Point p)
        {
            double[] points = new double[3];

            points[0] = p.X;
            points[1] = p.Y;
            points[2] = 0;

            toLatLon.TransformPoint(points);

            return new GeoCoordinate(points[1], points[0]);
        }

        // Does a transformation from Lat/Lon to 
        // spatial reference system

        public Point LatLon2coord(GeoCoordinate latlon)
        {
            double[] p = new double[3];

            p[0] = latlon.Longitude;
            p[1] = latlon.Latitude;
            p[2] = 0;

            toCoord.TransformPoint(p);

            return new Point(p[0], p[1]);
        }
        
        // Create a bitmap centered around the passed Lat/Lon

        public System.Drawing.Bitmap createBitMap(int width, int height, GeoCoordinate latlon)
        {
            // This code assumes single band raster with color palette
            if (ds.RasterCount != 1 || ds.GetRasterBand(1).GetRasterColorInterpretation() != ColorInterp.GCI_PaletteIndex)
            {
                throw new Exception("Single Raster Band of Palette Index Color is all that is supported");
            }

            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            // Get the raster x,y for the passed Lat/Lon
            Point rp = coord2px(LatLon2coord(latlon));
           
            byte[] colorIndexes = new byte[width * height];

            // Calculate offset from raster origin to bitmap origin
            xOffset = Convert.ToInt32(rp.X) - width / 2;
            yOffset = Convert.ToInt32(rp.Y) - height / 2;

            if ( xOffset < 0 || xOffset > this.width)
            {
                throw new Exception("Longitude out of range for raster image");
            }

            if ( yOffset < 0 || yOffset > this.height )
            {
                throw new Exception("Lattidue out of range for raster image");
            }

            // Read the color indexes
            ds.GetRasterBand(1).ReadRaster( xOffset, yOffset, width, height, colorIndexes, width, height, 0, 0);

            // Convert the color indexes to RGB and fill the bitmap
            ColorTable ct = ds.GetRasterBand(1).GetColorTable();

            for (int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    ColorEntry ce = ct.GetColorEntry(colorIndexes[i + j * width]);
 
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(
                        Convert.ToInt32(ce.c1),
                        Convert.ToInt32(ce.c2),
                        Convert.ToInt32(ce.c3)
                    );

                    bm.SetPixel(i, j, newColor);
                }
            }
                
            return bm;
        }
    }
}
