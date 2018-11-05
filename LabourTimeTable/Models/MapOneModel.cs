using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Echovoice.JSON;

namespace LabourTimeTable.Models
{
    public class MapOneModel
    {
        public string value { get; set; }
        public string id { get; set; }

        public MapOneModel(string _id, string _value)
        {
            value = _value;
            id = _id;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            sb.Append(JSONEncoders.EncodeJsString(id));
            sb.Append(',');
            sb.Append(value);
            sb.Append(']');

            return sb.ToString();
        }
    }
}