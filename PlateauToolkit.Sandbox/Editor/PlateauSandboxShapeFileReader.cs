using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxShapeFileReader : IDisposable
    {
        BinaryReader m_ShpReader;
        string m_ShpPath;

        enum ShapeType
        {
            Polygon = 5,
            Polyline = 3,
            Point = 1
        }
        public int ShapeConstants => m_ShapeType;
        int m_ShapeType;

        public PlateauSandboxShapeFileReader(string shpPath)
        {
            m_ShpPath = shpPath;
        }

        public List<IShape> ReadShapes()
        {
            using (FileStream fileStream = new FileStream(m_ShpPath, FileMode.Open))
            {
                m_ShpReader = new BinaryReader(fileStream);
                ReadHeader();
                List<IShape> shapes = new List<IShape>();

                while (m_ShpReader.BaseStream.Position < m_ShpReader.BaseStream.Length)
                {
                    m_ShpReader.ReadInt32BE(); // Record number
                    m_ShpReader.ReadInt32BE(); // Content length

                    m_ShapeType = m_ShpReader.ReadInt32();

                    if (m_ShapeType == (int)ShapeType.Polygon) // Cast to int since m_ShapeType is likely an int
                    {
                        shapes.Add(ReadPolygonShape());
                    }
                    else if (m_ShapeType == (int)ShapeType.Polyline)
                    {
                        shapes.Add(ReadPolylineShape());
                    }
                    else if (m_ShapeType == (int)ShapeType.Point)
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
            m_ShpReader.ReadInt32BE(); // File code
            m_ShpReader.ReadBytes(20); // Skip 20 bytes
            m_ShpReader.ReadInt32BE(); // File length
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
            PolylineShape shape = new PolylineShape();
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
                shape.Points.Add(new Vector3((float)x, 0f, (float)y));
            }


            return shape;
        }

        PointShape ReadPointShape()
        {
            // Read the X and Y coordinates for the point.
            double x = m_ShpReader.ReadDouble();
            double y = m_ShpReader.ReadDouble();

            // Create and return the PointShape.
            return new PointShape(new Vector3((float)x, 0f, (float)y));
        }


        PolygonShape ReadPolygonShape()
        {
            PolygonShape shape = new PolygonShape();
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
                shape.Points.Add(new Vector3((float)x, 0, (float)y));
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
        List<Vector3> Points { get; }
        List<int> Parts { get; }
        public string ToDataString()
        {
            return $"Points: num {Points.Count} : {string.Join(", ", Points.Select(p => $"({p.x}, {p.y}, {p.z})"))}," +
                   $"Parts: num {Parts.Count} : {string.Join(", ", Parts.Select(p => p.ToString()))}";
        }
    }
    class PolylineShape : IShape
    {
        public List<Vector3> Points { get; } = new List<Vector3>();
        public List<int> Parts { get; set; } = new List<int>();
    }

    class PolygonShape : IShape
    {
        public List<Vector3> Points { get; } = new List<Vector3>();
        public List<int> Parts { get; set; } = new List<int>();
    }

    class PointShape : IShape
    {
        // For a PointShape, we'll only ever have one point, but we're
        // implementing the IShape interface which requires a list.
        public List<Vector3> Points { get; } = new List<Vector3>();
        public List<int> Parts { get; } = new List<int>(); // Points do not have parts but we include this for interface compliance.

        public PointShape(Vector3 point)
        {
            Points.Add(point);
        }
    }

    public static class BinaryReaderExtensions
    {
        public static int ReadInt32BE(this BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            System.Array.Reverse(bytes);
            return System.BitConverter.ToInt32(bytes, 0);
        }
    }

}