using CsvHelper;
using CsvHelper.Configuration;
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
            var gpPath = @"C:\GitHub\Graph\TestData\GPs.csv";
            var gpReader = new GpReader(gpPath);

            gpReader.Read();
        }
    }


    class GpReader
    {
        private string filePath;

        public GpReader(string filePath)
        {
            this.filePath = filePath;
        }

        public void Read()
        {
            using (var file = new StreamReader(filePath))
            {
                var csv = new CsvReader(file);
                csv.Configuration.RegisterClassMap<GpRecordMap>();

                while (csv.Read())
                {
                    var record = csv.GetRecord<GpRecord>();

                    Console.WriteLine();
                }
            }
        }
    }

    public class GpRecord
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NationalGrouping { get; set; }
        public string HighLevelHealth { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string PostCode { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public char StatusCode { get; set; }
        public char TypeCode { get; set; }
        public string ParentCode { get; set; }
        public DateTime? JoinParentDate { get; set; }
        public DateTime? LeftParentDate { get; set; }
        public string ContactNo { get; set; }
        public bool Amended { get; set; }
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

            Map(m => m.OpenDate).Name("Open Date").ConvertUsing(row =>
            {
                var date = row.GetField<string>("Open Date");
                return GetGpDate(date);
            });

            Map(m => m.CloseDate).Name("Close Date").ConvertUsing(row =>
            {
                var date = row.GetField<string>("Close Date");
                return GetGpDate(date);
            });

            Map(m => m.StatusCode).Name("Status Code");
            Map(m => m.TypeCode).Name("Org Sub Type Code");
            Map(m => m.ParentCode).Name("Parent Org Code");

            Map(m => m.JoinParentDate).Name("Join Parent Date").ConvertUsing(row =>
            {
                var date = row.GetField<string>("Join Parent Date");
                return GetGpDate(date);
            });

            Map(m => m.LeftParentDate).Name("Left Parent Date").ConvertUsing(row =>
            {
                var date = row.GetField<string>("Left Parent Date");
                return GetGpDate(date);
            });

            Map(m => m.ContactNo).Name("Contact Tel Num");
            Map(m => m.Amended).Name("Amended Record").TypeConverterOption(true, "1");
            Map(m => m.CurrentCareOrg).Name("Current Care Org");
        }

        public static DateTime? GetGpDate(string date)
        {
            date = date.Trim();
            if (string.IsNullOrWhiteSpace(date) || date.Length != 8) return null;
            else
                return new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));
        }
    }
}
