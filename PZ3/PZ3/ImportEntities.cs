﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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



        public static void LoadAndParseXML(string location)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(location);
            XmlNodeList lineNodes = xmlDoc.SelectNodes("/NetworkModel/Lines/LineEntity");
            XmlNodeList substationNodes = xmlDoc.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            XmlNodeList switchNodes = xmlDoc.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            XmlNodeList nodeNodes = xmlDoc.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            lines = createLineEntities(lineNodes);
            substations = createSubstationEntities(substationNodes);
            switches = createSwitchEntities(switchNodes);
            nodes = createNodeEntities(nodeNodes);

        }

        private static List<LineEntity> createLineEntities(XmlNodeList lineNodes)
        {
            List<LineEntity> ret = new List<LineEntity>();

            foreach (XmlNode node in lineNodes)
            {
                LineEntity line = new LineEntity();
                line.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                line.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                line.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                line.IsUnderground = bool.Parse(node.SelectSingleNode("IsUnderground").InnerText);
                line.LineType = node.SelectSingleNode("LineType").InnerText;
                line.Name = node.SelectSingleNode("Name").InnerText;
                line.R = float.Parse(node.SelectSingleNode("R").InnerText);
                line.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);
                line.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);

                bool postoji_dupla_suprotni_smer = false;

                foreach (LineEntity templine in ret)
                {
                    if ((templine.FirstEnd == line.SecondEnd && templine.SecondEnd == line.FirstEnd) || (templine.FirstEnd == line.FirstEnd && templine.SecondEnd == line.SecondEnd))
                    {
                        postoji_dupla_suprotni_smer = true;
                    }
                }

                if (postoji_dupla_suprotni_smer == false)
                {
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
                ToLatLon(sub.X, sub.Y, 34, out noviX, out noviY);
                sub.X = noviX;
                sub.Y = noviY;
                ret.Add(sub);

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
                ToLatLon(swtch.X, swtch.Y, 34, out noviX, out noviY);
                swtch.X = noviX;
                swtch.Y = noviY;
                ret.Add(swtch);
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
                ToLatLon(nEntity.X, nEntity.Y, 34, out noviX, out noviY);
                nEntity.X = noviX;
                nEntity.Y = noviY;
                ret.Add(nEntity);
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


    }




}
