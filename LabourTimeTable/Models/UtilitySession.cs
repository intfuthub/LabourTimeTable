using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabourTimeTable.Models
{
    public class UtilitySession
    {
        public LabourDetailDTO Session
        {
            get { return Get<LabourDetailDTO>("LabourDetailDTO"); }
            set { Set<LabourDetailDTO>("LabourDetailDTO", value); }
        }

        private T Get<T>(string key)
        {
            object o = HttpContext.Current.Session[key];
            if (o is T)
            {
                return (T)o;
            }

            return default(T);
        }

        private void Set<T>(string key, T item)
        {
            HttpContext.Current.Session[key] = item;
        }
    }


    public class UtilityComplainSession
    {
        public LabourComplaintDetailDTO Session
        {
            get { return Get<LabourComplaintDetailDTO>("LabourComplaintDetailDTO"); }
            set { Set<LabourComplaintDetailDTO>("LabourComplaintDetailDTO", value); }
        }

        private T Get<T>(string key)
        {
            object o = HttpContext.Current.Session[key];
            if (o is T)
            {
                return (T)o;
            }

            return default(T);
        }

        private void Set<T>(string key, T item)
        {
            HttpContext.Current.Session[key] = item;
        }
    }



}