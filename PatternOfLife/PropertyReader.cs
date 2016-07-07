using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public class PropertyReader
    {
        private string filePath;
        private bool hasHeader;

        public PropertyReader(string filePath, bool hasHeader)
        {
            this.filePath = filePath;
            this.hasHeader = hasHeader;
        }

        public IEnumerable<PropTrans> Read()
        {
            using (var file = new StreamReader(filePath))
            {
                var csv = new CsvReader(file);
                csv.Configuration.RegisterClassMap<PropTransMap>();
                csv.Configuration.HasHeaderRecord = this.hasHeader;
                while (csv.Read())
                {
                    //PropTrans x;

                    //try
                    //{
                    //    x = csv.GetRecord<PropTrans>();
                    //}
                    //catch (Exception e)
                    //{
                    //    var y = e.Data["CsvHelper"];
                    //    throw;
                    //}


                    //yield return x;

                    yield return csv.GetRecord<PropTrans>();
                }
            }
        }
    }
}
