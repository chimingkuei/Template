using netDxf.Entities;
using netDxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DataNexus
{
    public class DXFWriter
    {
        public DxfDocument doc = new DxfDocument();

        public void CreateLine(Vector2 v1, Vector2 v2)
        {
            netDxf.Entities.Line entity = new netDxf.Entities.Line(v1, v2);
            doc.Entities.Add(entity);
        }

        public void CreateCircle(Vector3 v, double d)
        {
            netDxf.Entities.Circle entity = new netDxf.Entities.Circle { Center = v, Radius = d };
            entity.Color = netDxf.AciColor.Blue;
            doc.Entities.Add(entity);
        }

        public void CreatePolyline2D(Vector2[] points)
        {
            Polyline2D entity = new Polyline2D(points);
            entity.Color = netDxf.AciColor.Red;
            doc.Entities.Add(entity);
        }

        public void Save(string filepath)
        {
            doc.Save(filepath);
        }
    }

    class DXFReader
    {
        #region DXF Data Struct Class
        public class CircleStruct
        {
            public int circle { get; set; }
            public Vector3 center { get; set; }
            public double radius { get; set; }
        }
        public class LineStruct
        {
            public int line { get; set; }
            public Vector3 startpoint { get; set; }
            public Vector3 endpoint { get; set; }
        }
        public class Polylines2DStruct
        {
            public int polylines2D { get; set; }
            public double posX { get; set; }
            public double posY { get; set; }
        }
        #endregion
        public ObservableCollection<CircleStruct> circle { get; set; }
        public ObservableCollection<LineStruct> line { get; set; }
        public ObservableCollection<Polylines2DStruct> polylines2D { get; set; }

        private string[] GetListViewHeader(ListView listView)
        {
            List<string> headers = new List<string>();
            if (listView.View is GridView gridView)
            {
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (!string.IsNullOrEmpty(column.Header.ToString()))
                    {
                        headers.Add(column.Header.ToString());
                    }
                }
            }
            return headers.ToArray();
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                // 如果字段包含逗号、双引号或换行符，用双引号包围字段，并将双引号转义为两个双引号
                field = "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private List<string[]> GetListViewContent(ListView listView)
        {
            List<string[]> data = new List<string[]>();
            foreach (var item in listView.Items)
            {
                if (item is CircleStruct circle)
                {
                    string[] content = new string[3];
                    content[0] = circle.circle.ToString();
                    content[1] = EscapeCsvField(circle.center.ToString());
                    content[2] = circle.radius.ToString();
                    data.Add(content);
                }
                if (item is LineStruct line)
                {
                    string[] content = new string[3];
                    content[0] = line.line.ToString();
                    content[1] = EscapeCsvField(line.startpoint.ToString());
                    content[2] = EscapeCsvField(line.endpoint.ToString());
                    data.Add(content);
                }
                if (item is Polylines2DStruct polylines2D)
                {
                    string[] content = new string[3];
                    content[0] = polylines2D.polylines2D.ToString();
                    content[1] = polylines2D.posX.ToString();
                    content[2] = polylines2D.posY.ToString();
                    data.Add(content);
                }
            }
            return data;
        }

        public void ExportCsv(ListView listView, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(string.Join(",", GetListViewHeader(listView)));
                foreach (var row in GetListViewContent(listView))
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }




    }

}
