using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PZ3.Models
{
    class NodeEntity:PowerEntity
    {
        public NodeEntity()
        {

        }
        public override void SetElementColor()
        {
            PowerEntityShape.Fill = Brushes.Blue;
        }
        public override string printToolTip()
        {
            return "Type: Node entity" + Environment.NewLine + "ID: " + this.Id + Environment.NewLine + "Name: " + this.Name;
        }
    }
}
