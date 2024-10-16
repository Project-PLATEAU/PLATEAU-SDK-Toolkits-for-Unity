using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    public class PlateauSandboxShapefileReader : IDisposable
    {
        BinaryReader m_ShpReader;
        readonly string m_ShpPath;

        enum ShapeType
        {
            k_Polygon = 5,
            k_Polyline = 3,
            k_Point = 1
        }
        public int ShapeConstants => m_ShapeType;
        int m_ShapeType;

        public PlateauSandboxShapefileReader(string shpPath)
        {
            m_ShpPath = shpPath;
        }

        public List<IShape> ReadShapes()
        {
            using (var fileStream = new FileStream(m_ShpPath, FileMode.Open))
            {
                m_ShpReader = new BinaryReader(fileStream);
                ReadHeader();
                var shapes = new List<IShape>();

                while (m_ShpReader.BaseStream.Position < m_ShpReader.BaseStream.Length)
                {
                    m_ShpReader.ReadInt32Be(); // Record number
                    m_ShpReader.ReadInt32Be(); // Content length

                    m_ShapeType = m_ShpReader.ReadInt32();

                    if (m_ShapeType == (int)ShapeType.k_Polygon) // Cast to int since m_ShapeType is likely an int
                    {
                        shapes.Add(ReadPolygonShape());
                    }
                    else if (m_ShapeType == (int)ShapeType.k_Polyline)
                    {
                        shapes.Add(ReadPolylineShape());
                    }
                    else if (m_ShapeType == (int)ShapeType.k_Point)
                    {
                        shapes.Add(ReadPointShape());
                    }
                    else
                    {
                        Debug.LogError($"Unsupported shape type: {m_ShapeType}");
                        break;
                    }
                }
                return shapes;
            }
        }

        void ReadHeader()
        {
            m_ShpReader.BaseStream.Seek(0, SeekOrigin.Begin);
            m_ShpReader.ReadInt32Be(); // File code
            m_ShpReader.ReadBytes(20); // Skip 20 bytes
            m_ShpReader.ReadInt32Be(); // File length
            m_ShpReader.ReadInt32();   // Version
            m_ShpReader.ReadInt32();   // Shape type
            m_ShpReader.ReadDouble();  // X min
            m_ShpReader.ReadDouble();  // Y min
            m_ShpReader.ReadDouble();  // X max
            m_ShpReader.ReadDouble();  // Y max
            m_ShpReader.ReadDouble();  // Z min
            m_ShpReader.ReadDouble();  // Z max
            m_ShpReader.ReadDouble();  // M min
            m_ShpReader.ReadDouble();  // M max
        }
        PolylineShape ReadPolylineShape()
        {
            var shape = new PolylineShape();
            m_ShpReader.ReadBytes(32); // Bounding box (X min, Y min, X max, Y max)

            int numParts = m_ShpReader.ReadInt32();
            int numPoints = m_ShpReader.ReadInt32();
            int[] parts = new int[numParts + 1];
            for (int i = 0; i < numParts; i++)
            {
                parts[i] = m_ShpReader.ReadInt32();
                shape.Parts.Add(parts[i]);
            }
            parts[numParts] = numPoints;
            shape.Parts.Add(numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                double x = m_ShpReader.ReadDouble();
                double y = m_ShpReader.ReadDouble();
                shape.Points.Add((x, 0, y));
            }


            return shape;
        }

        PointShape ReadPointShape()
        {
            // Read the X and Y coordinates for the point.
            double x = m_ShpReader.ReadDouble();
            double y = m_ShpReader.ReadDouble();

            // Create and return the PointShape.
            return new PointShape((x, 0, y));
        }


        PolygonShape ReadPolygonShape()
        {
            var shape = new PolygonShape();
            m_ShpReader.ReadBytes(32); // Bounding box (X min, Y min, X max, Y max)

            int numParts = m_ShpReader.ReadInt32();
            int numPoints = m_ShpReader.ReadInt32();
            //Debug.Log("number of points: " + numPoints  );
            int[] parts = new int[numParts + 1];
            for (int i = 0; i < numParts; i++)
            {
                parts[i] = m_ShpReader.ReadInt32();
                shape.Parts.Add(parts[i]);
            }
            parts[numParts] = numPoints;
            shape.Parts.Add(numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                double x = m_ShpReader.ReadDouble();
                double y = m_ShpReader.ReadDouble();
                shape.Points.Add((x, 0, y));
            }
            return shape;
        }

        // Implement IDisposable
        public void Dispose()
        {
            if (m_ShpReader != null)
            {
                m_ShpReader.Close();
                m_ShpReader.Dispose();
                m_ShpReader = null;
            }
        }
    }

    public interface IShape
    {
        List<(double x, double y, double z)> Points { get; }
        List<int> Parts { get; }
        public string ToDataString()
        {
            return $"Points: num {Points.Count} : {string.Join(", ", Points.Select(p => $"({p.x}, {p.y}, {p.z})"))}," +
                   $"Parts: num {Parts.Count} : {string.Join(", ", Parts.Select(p => p.ToString()))}";
        }
    }
    class PolylineShape : IShape
    {
        public List<(double x, double y, double z)> Points { get; } = new List<(double x, double y, double z)>();
        public List<int> Parts { get; set; } = new List<int>();
    }

    class PolygonShape : IShape
    {
        public List<(double x, double y, double z)> Points { get; } = new List<(double x, double y, double z)>();
        public List<int> Parts { get; set; } = new List<int>();
    }

    class PointShape : IShape
    {
        // For a PointShape, we'll only ever have one point, but we're
        // implementing the IShape interface which requires a list.
        public List<(double x, double y, double z)> Points { get; } = new List<(double x, double y, double z)>();
        public List<int> Parts { get; } = new List<int>(); // Points do not have parts But we include this for interface compliance.

        public PointShape((double, double, double) point)
        {
            Points.Add(point);
        }
    }

    public static class BinaryReaderExtensions
    {
        public static int ReadInt32Be(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }

}