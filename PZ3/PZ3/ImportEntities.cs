using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;
using PZ3.Models;
namespace PZ3
{
    static class ImportEntities
    {
        public static List<LineEntity> lines = new List<LineEntity>();
        public static List<SubstationEntity> substations = new List<SubstationEntity>();
        public static List<SwitchEntity> switches = new List<SwitchEntity>();
        public static List<NodeEntity> nodes = new List<NodeEntity>();

        public const double MAX_LAT = 45.277031;
        public const double MAX_LON = 19.894459;
        public const double MIN_LAT = 45.2325;
        public const double MIN_LON = 19.793909;
        public const double divides_X = (MAX_LON - MIN_LON) / 10; //10x10 je mapa znaci imacemo 10 jednakih podeoka
        public const double divides_Y = (MAX_LAT - MIN_LAT) / 10;


        public static void LoadAndParseXML(string location)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(location);
            XmlNodeList lineNodes = xmlDoc.SelectNodes("/NetworkModel/Lines/LineEntity");
            XmlNodeList substationNodes = xmlDoc.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            XmlNodeList switchNodes = xmlDoc.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            XmlNodeList nodeNodes = xmlDoc.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            
            substations = createSubstationEntities(substationNodes);
            switches = createSwitchEntities(switchNodes);
            nodes = createNodeEntities(nodeNodes);
            lines = createLineEntities(lineNodes);

        }

        private static List<LineEntity> createLineEntities(XmlNodeList lineNodes)
        {
            List<LineEntity> ret = new List<LineEntity>();

            foreach (XmlNode node in lineNodes)
            {
                LineEntity line = new LineEntity();
                line.Vertices = new List<Point3D>();
                line.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                line.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                line.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                line.IsUnderground = bool.Parse(node.SelectSingleNode("IsUnderground").InnerText);
                line.LineType = node.SelectSingleNode("LineType").InnerText;
                line.Name = node.SelectSingleNode("Name").InnerText;
                line.R = float.Parse(node.SelectSingleNode("R").InnerText,CultureInfo.InvariantCulture);
                line.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);
                line.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);

                bool ignore = false;

                //da bi znali da liniju treba crtati treba pronaci njen pocetak i kraj tj sta povezuje, ako ne pronadjemo znaci da je van opsega i ignorisemo liniju
                PowerEntity startEntity = null;
                PowerEntity endEntity = null;
                if((switches.Find(x => x.Id == line.FirstEnd)) == null)
                {

                    if ((nodes.Find(x => x.Id == line.FirstEnd)) == null)
                    {
                        if ((substations.Find(x => x.Id == line.FirstEnd)) == null)
                        {
                            ignore = true;
                        }
                        else
                        {
                            startEntity = substations.Find(x => x.Id == line.FirstEnd);
                            startEntity.NumberOfConnections = startEntity.NumberOfConnections + 1;
                        }
                    }
                    else
                    {
                        startEntity = nodes.Find(x => x.Id == line.FirstEnd);
                        startEntity.NumberOfConnections = startEntity.NumberOfConnections + 1;
                    }
                }
                else
                {
                    startEntity = switches.Find(x => x.Id == line.FirstEnd);
                    startEntity.NumberOfConnections = startEntity.NumberOfConnections + 1;
                    if((startEntity as SwitchEntity).Status == "Open")
                    {
                        line.Active = false;
                    }
                }
                //ako smo pronasli pocetnu tacku tek ima smisla traziti kraj
                if(startEntity != null)
                {
                    if ((switches.Find(x => x.Id == line.SecondEnd)) == null)
                    {

                        if ((nodes.Find(x => x.Id == line.SecondEnd)) == null)
                        {
                            if ((substations.Find(x => x.Id == line.SecondEnd)) == null)
                            {
                                ignore = true;
                            }
                            else
                            {
                                endEntity = substations.Find(x => x.Id == line.SecondEnd);
                                endEntity.NumberOfConnections = endEntity.NumberOfConnections + 1;
                            }
                        }
                        else
                        {
                            endEntity = nodes.Find(x => x.Id == line.SecondEnd);
                            endEntity.NumberOfConnections = endEntity.NumberOfConnections + 1;
                        }
                    }
                    else
                    {
                        endEntity = switches.Find(x => x.Id == line.SecondEnd);
                        endEntity.NumberOfConnections = endEntity.NumberOfConnections + 1;
                        if ((endEntity as SwitchEntity).Status == "Open")
                        {
                            line.Active = false;
                        }
                    }
                }

                

                if (ignore == false)
                {

                    foreach (XmlNode point in node.SelectSingleNode("Vertices"))
                    {
                        double x, y;
                        ToLatLon(double.Parse(point.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture), double.Parse(point.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture), 34, out y, out x);
                        //bilo je linija koje su se prostirale van mape zbog vertices koje su spajale tacke koje smo izignorisali
                        if (x < MIN_LON || x > MAX_LON || y > MAX_LAT || y < MIN_LAT)
                        {
                            continue;
                        }
                        else {
                            line.Vertices.Add(new Point3D(x, y, 1));
                        }
                        
                    }
                    //u xmlu ako linija ima tacno 2 vertices to su koordinate sta ona spaja, ipak ako ima vise to su samo koordinate medjutacaka pa moramo dodati i koordinate elementa tj prve tacke linije i poslednje

                    if(line.Vertices.Count > 2)
                    {
                        line.Vertices.Insert(0, new Point3D(startEntity.X, startEntity.Y, 1));
                        line.Vertices.Add(new Point3D(endEntity.X, endEntity.Y, 1));
                    }
                   





                    ret.Add(line);
                }

            }

            return ret;
        }


        private static List<SubstationEntity> createSubstationEntities(XmlNodeList substationNodes)
        {
            List<SubstationEntity> ret = new List<SubstationEntity>();
            foreach (XmlNode node in substationNodes)
            {

                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);


                double noviX, noviY;
                ToLatLon(sub.X, sub.Y, 34, out noviY, out noviX);
                sub.X = noviX;
                sub.Y = noviY;

                bool ignore = false;

                if (sub.X < MIN_LON || sub.X > MAX_LON || sub.Y > MAX_LAT || sub.Y < MIN_LAT)
                {
                    ignore = true; //izignorisemo
                }

                if(ignore == false)
                {
                    ret.Add(sub);
                }
                

            }
            return ret;
        }

        private static List<SwitchEntity> createSwitchEntities(XmlNodeList switchNodes)
        {
            List<SwitchEntity> ret = new List<SwitchEntity>();
            foreach (XmlNode node in switchNodes)
            {
                SwitchEntity swtch = new SwitchEntity();
                swtch.Id = long.Parse(node.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture);
                swtch.Name = node.SelectSingleNode("Name").InnerText;
                swtch.Status = node.SelectSingleNode("Status").InnerText;
                swtch.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                swtch.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);
                double noviX, noviY;
                ToLatLon(swtch.X, swtch.Y, 34, out noviY, out noviX);
                swtch.X = noviX;
                swtch.Y = noviY;
                


                bool ignore = false;

                if (swtch.X < MIN_LON || swtch.X > MAX_LON || swtch.Y > MAX_LAT || swtch.Y < MIN_LAT)
                {
                    ignore = true; //izignorisemo
                }

                if (ignore == false)
                {
                    ret.Add(swtch);
                }
            }


            return ret;
        }

        private static List<NodeEntity> createNodeEntities(XmlNodeList nodeNodes)
        {
            List<NodeEntity> ret = new List<NodeEntity>();
            foreach (XmlNode node in nodeNodes)
            {
                NodeEntity nEntity = new NodeEntity();
                nEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture);
                nEntity.Name = node.SelectSingleNode("Name").InnerText;
                nEntity.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                nEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);
                double noviX, noviY;
                ToLatLon(nEntity.X, nEntity.Y, 34, out noviY, out noviX);
                nEntity.X = noviX;
                nEntity.Y = noviY;


                bool ignore = false;

                if (nEntity.X < MIN_LON || nEntity.X > MAX_LON || nEntity.Y > MAX_LAT || nEntity.Y < MIN_LAT)
                {
                    ignore = true; //izignorisemo
                }

                if (ignore == false)
                {
                    ret.Add(nEntity);
                }
            }



            return ret;
        }


        //From UTM to Latitude and longitude in decimal
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }


        //Ne zanima me vise apsolutna LAT/LON sada moram gledati odnos na referentne minimume zadatke u zadatku
        public static void ScaleLatLon()
        {
            foreach(SubstationEntity sub in substations)
            {
                sub.X = (sub.X - MIN_LON) / divides_X; //dobicemo broj podeoka po X-u zarez ostatak
                sub.Y = (sub.Y - MIN_LAT) / divides_Y; //##//

            }

            foreach (NodeEntity sub in nodes)
            {
                sub.X = (sub.X - MIN_LON) / divides_X; 
                sub.Y = (sub.Y - MIN_LAT) / divides_Y; 

            }

            foreach (SwitchEntity sub in switches)
            {
                sub.X = (sub.X - MIN_LON) / divides_X; //dobicemo broj podeoka po X-u zarez ostatak
                sub.Y = (sub.Y - MIN_LAT) / divides_Y; //##//

            }

            foreach(LineEntity line in lines)
            {

                for(int i =0; i < line.Vertices.Count; i++)
                {
                    double newX = (line.Vertices[i].X - MIN_LON) / divides_X;
                    double newY = (line.Vertices[i].Y - MIN_LAT) / divides_Y;
                    line.Vertices[i] = new Point3D(newX, newY, 1);
                }
            }
        }

    }




}
