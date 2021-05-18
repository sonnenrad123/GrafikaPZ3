using PZ3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PZ3
{
    static class ModelCreator
    {
        public static Model3DGroup groupModels;
        public const double dimensions = 0.05;
        public const double line_dimensions = 0.02;

        public static void CreateModels(Model3DGroup modelGroup, List<SubstationEntity> substations, List<SwitchEntity> switches,List<LineEntity> lines,List<NodeEntity> nodes)
        {
            groupModels = modelGroup;
            foreach(SubstationEntity tmp in substations)
            {
                CreateCube(tmp);
           
            }

            foreach (NodeEntity tmp in nodes)
            {
                CreateCube(tmp);

            }

            foreach (SwitchEntity tmp in switches)
            {
                CreateCube(tmp);

            }
            foreach(LineEntity line in lines)
            {
                CreateConnector(line);
            }

        }



        private static void CreateCube(PowerEntity ent)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection positions = new Point3DCollection();

            //donja stranica
            positions.Add(new Point3D(ent.X, ent.Y, dimensions));
            positions.Add(new Point3D(ent.X + dimensions, ent.Y, dimensions));
            positions.Add(new Point3D(ent.X + dimensions, ent.Y + dimensions, dimensions));
            positions.Add(new Point3D(ent.X, ent.Y + dimensions, dimensions));
            //gornja stranica
            positions.Add(new Point3D(ent.X, ent.Y, dimensions*2));
            positions.Add(new Point3D(ent.X + dimensions, ent.Y, dimensions*2));
            positions.Add(new Point3D(ent.X + dimensions, ent.Y + dimensions, dimensions*2));
            positions.Add(new Point3D(ent.X, ent.Y + dimensions, dimensions*2));
            //sada imam sve tacke, mogu da proverim preseke
            mesh.Positions = positions;
            foreach(var chldr in groupModels.Children)
            {
                while (true) 
                {
                    if (mesh.Bounds.IntersectsWith(chldr.Bounds))//ako se podudaraju liftujemo kocku na vec postojecu
                    {
                        for(int i = 0; i < mesh.Positions.Count; i++)
                        {
                            mesh.Positions[i] = new Point3D(mesh.Positions[i].X, mesh.Positions[i].Y, mesh.Positions[i].Z + 0.02);
                        }
                    }
                    else
                    {
                        break;//inace unisti infinit
                    }
                }
            }
            //trouglovi

            //donja stranica
            Int32Collection TriangleCollection = new Int32Collection();

           
            TriangleCollection.Add(1);
            TriangleCollection.Add(0);
            TriangleCollection.Add(3);
            TriangleCollection.Add(1);
            TriangleCollection.Add(3);
            TriangleCollection.Add(2);
            //gornja stranica
            TriangleCollection.Add(4);
            TriangleCollection.Add(5);
            TriangleCollection.Add(6);
            TriangleCollection.Add(4);
            TriangleCollection.Add(6);
            TriangleCollection.Add(7);
            //leva
            TriangleCollection.Add(0);
            TriangleCollection.Add(4);
            TriangleCollection.Add(7);
            TriangleCollection.Add(0);
            TriangleCollection.Add(7);
            TriangleCollection.Add(3);
            //desna
            TriangleCollection.Add(5);
            TriangleCollection.Add(1);
            TriangleCollection.Add(2);
            TriangleCollection.Add(5);
            TriangleCollection.Add(2);
            TriangleCollection.Add(6);
            //zadnja
            TriangleCollection.Add(3);
            TriangleCollection.Add(2);
            TriangleCollection.Add(6);
            TriangleCollection.Add(3);
            TriangleCollection.Add(6);
            TriangleCollection.Add(7);
            //prednja
            TriangleCollection.Add(1);
            TriangleCollection.Add(0);
            TriangleCollection.Add(4);
            TriangleCollection.Add(1);
            TriangleCollection.Add(4);
            TriangleCollection.Add(5);
            



            mesh.TriangleIndices = TriangleCollection;
            ent.SetElementColor();
            GeometryModel3D gModel = new GeometryModel3D(mesh, new DiffuseMaterial(ent.PowerEntityShape.Fill));
            gModel.SetValue(FrameworkElement.TagProperty, ent);
            groupModels.Children.Add(gModel);

        }


        private static void CreateConnector(LineEntity line)
        {
            for(int i = 0; i < line.Vertices.Count - 1; i++)//ideja je da uzimamo dve po dve tacke i crtamo deo po deo linije uz pomoc trouglova
            {
                CreateConnectorParts(line.Vertices[i], line.Vertices[i + 1], line);
            }

        }

        private static void CreateConnectorParts(Point3D start, Point3D end, LineEntity lineBelongingTo)
        {
            double height_for_line_drawing = dimensions/1.5;

            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection PositionCollection = new Point3DCollection();
            /*
             Ideja je da prosirim svaku koord dodatih tacaka za neku mizernu vrednost koja nece uticati na preciznost znatno, tj bice prekrivena kockom ili nastavkom linije
             bez da razmisljam da li je linija dijagonalna, vodoravna, horizont i td..
             */
             PositionCollection.Add(new Point3D(start.X, start.Y, height_for_line_drawing));
            PositionCollection.Add(new Point3D(start.X+line_dimensions, start.Y+line_dimensions,height_for_line_drawing));
            PositionCollection.Add(new Point3D(end.X,end.Y,height_for_line_drawing));
            PositionCollection.Add(new Point3D(end.X + line_dimensions, end.Y + dimensions, height_for_line_drawing));
            PositionCollection.Add(new Point3D(start.X + line_dimensions/2, start.Y + line_dimensions/2, height_for_line_drawing+line_dimensions));
            PositionCollection.Add(new Point3D(end.X + line_dimensions/2, end.Y + dimensions/2, height_for_line_drawing+line_dimensions));

            mesh.Positions = PositionCollection;

            Int32Collection TriangleCollection = new Int32Collection();
            //1 stranica
            TriangleCollection.Add(3);
            TriangleCollection.Add(2);
            TriangleCollection.Add(0);

            TriangleCollection.Add(3);
            TriangleCollection.Add(0);
            TriangleCollection.Add(1);

            //2 stranica
            TriangleCollection.Add(2);
            TriangleCollection.Add(5);
            TriangleCollection.Add(4);

            TriangleCollection.Add(2);
            TriangleCollection.Add(4);
            TriangleCollection.Add(0);

            //3 stranica
            TriangleCollection.Add(5);
            TriangleCollection.Add(3);
            TriangleCollection.Add(1);

            TriangleCollection.Add(5);
            TriangleCollection.Add(1);
            TriangleCollection.Add(4);


            mesh.TriangleIndices = TriangleCollection;

            GeometryModel3D ret = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Goldenrod));
            ret.SetValue(FrameworkElement.TagProperty, lineBelongingTo);
            groupModels.Children.Add(ret);

            
        }
    }
}
