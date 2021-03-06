﻿using Microsoft.Data.Sqlite;
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
using System.Windows.Threading;
using ThinkGeo.Core;
using ThinkGeo.UI.WinForms;


namespace MBTilesExtractor
{
    public partial class Form1 : Form
    {
        private const long maxZoom = 14;
        private LayerOverlay layerOverlay;
        string mbtilesPathFilename = "Data/NewYorkCity.mbtiles";
        string defaultJsonFilePath = "Data/thinkgeo-world-streets-light.json";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Map1.MapUnit = GeographyUnit.Meter;
            ThinkGeoMBTilesLayer thinkGeoMBTilesFeatureLayer = new ThinkGeoMBTilesLayer(mbtilesPathFilename, new Uri(defaultJsonFilePath, UriKind.Relative));

            layerOverlay = new LayerOverlay();
            layerOverlay.Layers.Add(thinkGeoMBTilesFeatureLayer);
            this.Map1.Overlays.Add(layerOverlay);
            this.Map1.CurrentExtent = new RectangleShape(-8241733.850165642, 4972227.169416549, -8232561.406822379, 4967366.252176043);

             this.Map1.TrackOverlay.TrackEnded += TrackOverlay_TrackEnded;
        }

        private void TrackOverlay_TrackEnded(object sender, TrackEndedTrackInteractiveOverlayEventArgs e)
        {
            InMemoryFeatureLayer trackShapeLayer = Map1.TrackOverlay.TrackShapeLayer;

            string localFilePath;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MB Tiles Database(*.mbtiles)|*.mbtiles";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                localFilePath = saveFileDialog.FileName.ToString();
                if (File.Exists(localFilePath)) File.Delete(localFilePath);

                PolygonShape extractingPolygon = new PolygonShape(trackShapeLayer.InternalFeatures.First().GetWellKnownBinary());

                Map1.TrackOverlay.TrackMode = TrackMode.None;
                ExtractTiles(extractingPolygon, localFilePath);
                trackShapeLayer.InternalFeatures.Clear();
                Map1.Refresh(Map1.TrackOverlay);
                Map1.TrackOverlay.TrackMode = TrackMode.Rectangle;

                ThinkGeoMBTilesLayer thinkGeoMBTilesFeatureLayer = new ThinkGeoMBTilesLayer(localFilePath, new Uri(defaultJsonFilePath, UriKind.Relative));
                thinkGeoMBTilesFeatureLayer.BitmapTileCache = null;
                layerOverlay.Layers.Clear();
                layerOverlay.Layers.Add(thinkGeoMBTilesFeatureLayer);
                chkExtractingData.Checked = false;
                Map1.Refresh(layerOverlay);
            }
            else
                trackShapeLayer.InternalFeatures.Clear();
        }

        private void ExtractTiles(PolygonShape polygonShape, string targetFilePath)
        {
            RectangleShape bbox = polygonShape.GetBoundingBox();
            List<VectorTileRange> tileRanges = new List<VectorTileRange>();
            for (int zoomLevel = 0; zoomLevel <= maxZoom; zoomLevel++)
            {
                VectorTileRange tileRange = GetTileRange(zoomLevel, bbox);
                tileRanges.Add(tileRange);
            }
            ThinkGeoMBTilesLayer.CreateDatabase(targetFilePath);

            var targetDBConnection = new SqliteConnection($"Data Source={targetFilePath}");
            targetDBConnection.Open();
            var targetMap = new TilesTable(targetDBConnection);
            var targetMetadata = new MetadataTable(targetDBConnection);

            var sourceDBConnection = new SqliteConnection($"Data Source={mbtilesPathFilename}");
            sourceDBConnection.Open();
            var sourceMap = new TilesTable(sourceDBConnection);
            var sourceMetadata = new MetadataTable(sourceDBConnection);

            sourceMetadata.ReadAllEntries();

            ProjectionConverter projection = new ProjectionConverter(3857, 4326);
            projection.Open();
            var wgs84BBox = projection.ConvertToExternalProjection(bbox);
            foreach (MetadataEntry entry in sourceMetadata.Entries)
            {
                if (entry.Name.Equals("center"))
                {
                    PointShape centerPoint = wgs84BBox.GetCenterPoint();
                    entry.Value = $"{centerPoint.X},{centerPoint.Y},{maxZoom}";
                }
                else if (entry.Name.Equals("bounds"))
                {
                    entry.Value = $"{wgs84BBox.UpperLeftPoint.X},{wgs84BBox.UpperLeftPoint.Y},{wgs84BBox.LowerRightPoint.X},{wgs84BBox.LowerRightPoint.Y}";
                }
            }
            targetMetadata.Insert(sourceMetadata.Entries);

            int recordLimit = 1000;
            foreach (var tileRange in tileRanges)
            {
                long offset = 0;
                bool isEnd = false;
                while (!isEnd)
                {
                    string querySql = $"SELECT * FROM {sourceMap.TableName} WHERE " + ConvetToSqlString(tileRange) + $" LIMIT {offset},{recordLimit}";
                    var entries = sourceMap.Query(querySql);
                    for (int i = entries.Count - 1; i >= 0; i--)
                    {
                        RectangleShape pbfExtent = GetPbfTileExent((int)entries[i].ZoomLevel, entries[i].TileColumn, entries[i].TileRow);
                        if (polygonShape.IsDisjointed(pbfExtent))
                        {
                            entries.RemoveAt(i);
                        }
                    }
                    targetMap.Insert(entries);

                    if (entries.Count < recordLimit)
                        isEnd = true;

                    offset = offset + recordLimit;
                }
            }
        }

        private static RectangleShape GetPbfTileExent(int zoom, long column, long row)
        {
            long rowCount = (long)Math.Pow(2, zoom);

            double cellWidth = MaxExtents.SphericalMercator.Width / rowCount;
            double cellHeight = MaxExtents.SphericalMercator.Height / rowCount;

            PointShape upperLeftPoint = new PointShape(MaxExtents.SphericalMercator.UpperLeftPoint.X + cellWidth * column, MaxExtents.SphericalMercator.LowerLeftPoint.Y + cellHeight * (row + 1));
            PointShape lowerRightPoint = new PointShape(upperLeftPoint.X + cellWidth, upperLeftPoint.Y - cellHeight);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
        }

        private string ConvetToSqlString(VectorTileRange tileRange)
        {
            long rowCount = (long)Math.Pow(2, tileRange.Zoom) - 1;

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append(TilesTable.ZoomLevelColumnName);
            sqlStringBuilder.Append(" = ");
            sqlStringBuilder.Append(tileRange.Zoom);
            sqlStringBuilder.Append(" AND ");
            sqlStringBuilder.Append(TilesTable.TileColColumnName);
            sqlStringBuilder.Append(" >= ");
            sqlStringBuilder.Append(tileRange.MinColumn);
            sqlStringBuilder.Append(" AND ");
            sqlStringBuilder.Append(TilesTable.TileColColumnName);
            sqlStringBuilder.Append(" <= ");
            sqlStringBuilder.Append(tileRange.MaxColumn);
            sqlStringBuilder.Append(" AND ");
            sqlStringBuilder.Append(TilesTable.TileRowColumnName);
            sqlStringBuilder.Append(" >= ");
            sqlStringBuilder.Append(rowCount - tileRange.MaxRow);
            sqlStringBuilder.Append(" AND ");
            sqlStringBuilder.Append(TilesTable.TileRowColumnName);
            sqlStringBuilder.Append(" <= ");
            sqlStringBuilder.Append(rowCount - tileRange.MinRow);

            return sqlStringBuilder.ToString();
        }

        private VectorTileRange GetTileRange(int zoomLevel, RectangleShape extent)
        {
            RectangleShape worldExtent = new RectangleShape(-20037508.2314698, 20037508.2314698, 20037508.2314698, -20037508.2314698);

            extent = worldExtent.GetIntersection(extent);

            long rowCount = (long)Math.Pow(2, zoomLevel);
            double cellWidth = worldExtent.Width / rowCount;
            double cellHeight = worldExtent.Height / rowCount;
            long minX = (long)Math.Floor((extent.UpperLeftPoint.X - worldExtent.UpperLeftPoint.X) / cellWidth);
            long minY = (long)Math.Floor((worldExtent.UpperLeftPoint.Y - extent.UpperLeftPoint.Y) / cellHeight);
            long maxX = (long)Math.Floor((extent.UpperRightPoint.X - worldExtent.UpperLeftPoint.X) / cellWidth);
            long maxY = (long)Math.Floor((worldExtent.UpperLeftPoint.Y - extent.LowerLeftPoint.Y) / cellHeight);

            return new VectorTileRange(zoomLevel, minX, minY, maxX, maxY);
        }

        private void chkExtractingData_CheckedChanged(object sender, EventArgs e)
        {
            Map1.TrackOverlay.TrackMode = chkExtractingData.Checked ? TrackMode.Polygon : TrackMode.None;

        }
    }
}
