using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public sealed class GpRecordMap : CsvClassMap<GpRecord>
    {
        public GpRecordMap()
        {
            Map(m => m.Code).Name("Organisation Code");
            Map(m => m.Name).Name("Name");
            Map(m => m.NationalGrouping).Name("National Grouping").ConvertUsing(row => row.GetField<string>("National Grouping").Trim());
            Map(m => m.HighLevelHealth).Name("High Level Health").ConvertUsing(row => row.GetField<string>("High Level Health").Trim());
            Map(m => m.Address1).Name("Address Line 1");
            Map(m => m.Address2).Name("Address Line 2");
            Map(m => m.Address3).Name("Address Line 3");
            Map(m => m.Address4).Name("Address Line 4");
            Map(m => m.Address5).Name("Address Line 5");
            Map(m => m.PostCode).Name("Post Code").ConvertUsing(row => row.GetField<string>("Post Code").Trim());
            Map(m => m.PostCodeInit).ConvertUsing(row => row.GetField<string>("Post Code").Trim().Split(' ')[0]);
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
