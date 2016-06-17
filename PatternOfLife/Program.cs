using CsvHelper;
using CsvHelper.Configuration;
using Neo4jClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), username: "neo4j", password: "qwerty123");
            client.Connect();

            var gpPath = @"Data\GPs.csv";
            var gpReader = new GpReader(gpPath);
            var gps = gpReader.Read();

            foreach (var item in gps)
            {
                client.Cypher.Create("(foo:GP {newGP})")
                    .WithParam("newGP", item)
                    .ExecuteWithoutResults();
            }
        }
    }


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

    public class GpRecord

    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "grouping")]
        public string NationalGrouping { get; set; }

        [JsonProperty(PropertyName = "health")]
        public string HighLevelHealth { get; set; }

        [JsonProperty(PropertyName = "add1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "add2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "add3")]
        public string Address3 { get; set; }

        [JsonProperty(PropertyName = "add4")]
        public string Address4 { get; set; }

        [JsonProperty(PropertyName = "add5")]
        public string Address5 { get; set; }

        [JsonProperty(PropertyName = "postcode")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "opendate")]
        public DateTime? OpenDate { get; set; }

        [JsonProperty(PropertyName = "closedate")]
        public DateTime? CloseDate { get; set; }

        [JsonProperty(PropertyName = "status")]
        public char StatusCode { get; set; }

        [JsonProperty(PropertyName = "type")]
        public char TypeCode { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public string ParentCode { get; set; }

        [JsonProperty(PropertyName = "joindate")]
        public DateTime? JoinParentDate { get; set; }

        [JsonProperty(PropertyName = "leftdate")]
        public DateTime? LeftParentDate { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string ContactNo { get; set; }

        [JsonProperty(PropertyName = "amended")]
        public bool Amended { get; set; }

        [JsonProperty(PropertyName = "careorg")]
        public string CurrentCareOrg { get; set; }
    }

    public sealed class GpRecordMap : CsvClassMap<GpRecord>
    {
        public GpRecordMap()
        {
            Map(m => m.Code).Name("Organisation Code");
            Map(m => m.Name).Name("Name");
            Map(m => m.NationalGrouping).Name("National Grouping");
            Map(m => m.HighLevelHealth).Name("High Level Health");
            Map(m => m.Address1).Name("Address Line 1");
            Map(m => m.Address2).Name("Address Line 2");
            Map(m => m.Address3).Name("Address Line 3");
            Map(m => m.Address4).Name("Address Line 4");
            Map(m => m.Address5).Name("Address Line 5");
            Map(m => m.PostCode).Name("Post Code");
            Map(m => m.OpenDate).Name("Open Date").ConvertUsing(row => GetDateField(row, "Open Date"));
            Map(m => m.CloseDate).Name("Close Date").ConvertUsing(row => GetDateField(row, "Close Date"));
            Map(m => m.StatusCode).Name("Status Code");
            Map(m => m.TypeCode).Name("Org Sub Type Code");
            Map(m => m.ParentCode).Name("Parent Org Code");
            Map(m => m.JoinParentDate).Name("Join Parent Date").ConvertUsing(row => GetDateField(row, "Join Parent Date"));
            Map(m => m.LeftParentDate).Name("Left Parent Date").ConvertUsing(row => GetDateField(row, "Left Parent Date"));
            Map(m => m.ContactNo).Name("Contact Tel Num");
            Map(m => m.Amended).Name("Amended Record").TypeConverterOption(true, "1");
            Map(m => m.CurrentCareOrg).Name("Current Care Org");
        }

        public static DateTime? GetDateField(ICsvReaderRow row, string fieldName)
        {
            var date = row.GetField<string>(fieldName).Trim();
            if (string.IsNullOrEmpty(date) || date.Length != 8) return null;
            else
                return new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));
        }
    }
}
