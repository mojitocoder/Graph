using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public class GpReader
    {
        private string filePath;

        public GpReader(string filePath)
        {
            this.filePath = filePath;
        }

        public IEnumerable<GpRecord> Read()
        {
            using (var file = new StreamReader(filePath))
            {
                var csv = new CsvReader(file);
                csv.Configuration.RegisterClassMap<GpRecordMap>();
                while (csv.Read())
                {
                    yield return csv.GetRecord<GpRecord>();
                }
            }
        }
    }
}
