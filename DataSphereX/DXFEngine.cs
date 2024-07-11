using netDxf.Entities;
using netDxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataSphereX
{
    class DXFWriter
    {
        public void CreateLine(DxfDocument doc, Vector2 v1, Vector2 v2)
        {
            netDxf.Entities.Line entity = new netDxf.Entities.Line(v1, v2);
            doc.Entities.Add(entity);
        }

        public void CreateCircle(DxfDocument doc, Vector3 v, double d)
        {
            netDxf.Entities.Circle entity = new netDxf.Entities.Circle { Center = v, Radius = d };
            entity.Color = netDxf.AciColor.Blue;
            doc.Entities.Add(entity);
        }

        public void CreatePolyline2D(DxfDocument doc, Vector2[] points)
        {
            Polyline2D entity = new Polyline2D(points);
            entity.Color = netDxf.AciColor.Red;
            doc.Entities.Add(entity);
        }
    }

    

}
