﻿using System;
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

        public void pixel2coordinate(double xp, double yp, out double xc, out double yc)
        {
            xc = GeoTransform[0] + (xp * GeoTransform[1]);
            yc = GeoTransform[3] + (yp * GeoTransform[5]);
        }


        // Does a transformation between display points and
        /// raster points

        public Point px2Map(Point dp, double displayWidth, double displayHeight)
        {
            double xScale = width / displayWidth;
            double yScale = height / displayHeight;
            return new Point(dp.X * xScale, dp.Y * yScale);
        }

        // Does a transformation between display points
        // and east,north point in the rasters spatial
        // system

        public Point px2coord(Point dp, double displayWidth, double displayHeight)
        {
            // calculate scaling to go from display to raster
            Point mp = new Point();
            mp.X = dp.X * (this.width / displayWidth);
            mp.Y = dp.Y * (this.height / displayHeight);

            Point v = new Point();
            v.X = GeoTransform[0] + (mp.X * GeoTransform[1]);
            v.Y = GeoTransform[3] + (mp.Y * GeoTransform[5]);

            return v;
        }

        // Does a transformation from spatial reference system
        // to display x,y

        public Point coord2px(Point p)
        {
            Point v = new Point();
            v.X = ((p.X - GeoTransform[0]) / GeoTransform[1]);
            v.Y = ((p.Y - GeoTransform[3]) / GeoTransform[5]);
            return v;
        }

        
        public void coordinate2LatLon(double xc, double yc, out double lat, out double lon)
        {
            double[] p = new double[3];

            p[0] = xc;
            p[1] = yc;
            p[2]  = 0;

            toLatLon.TransformPoint(p);

            lon = p[0];
            lat = p[1];
        }


        public GeoCoordinate coord2LatLon(Point p)
        {
            double[] points = new double[3];

            points[0] = p.X;
            points[1] = p.Y;
            points[2] = 0;

            toLatLon.TransformPoint(points);

            return new GeoCoordinate(points[1], points[0]);
        }

        public Point LatLon2coord(GeoCoordinate latlon)
        {
            double[] p = new double[3];

            p[0] = latlon.Longitude;
            p[1] = latlon.Latitude;
            p[2] = 0;

            toCoord.TransformPoint(p);

            return new Point(p[0], p[1]);
        }
        
    }
}
