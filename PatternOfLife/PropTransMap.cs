using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternOfLife
{
    public sealed class PropTransMap : CsvClassMap<PropTrans>
    {
        public PropTransMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.Price).Index(1);
            Map(m => m.DateOfTransfer).Index(2);
            Map(m => m.PostCode).Index(3).ConvertUsing(row => row.GetField<string>(3).Trim());
            Map(m => m.PostCodeInit).ConvertUsing(row => row.GetField<string>(3).Trim().Split(' ')[0]);
            Map(m => m.PropertyType).Index(4);
            Map(m => m.NewBuilding).Index(5).TypeConverterOption(true, "Y");
            Map(m => m.Freehold).Index(6).TypeConverterOption(true, "F").TypeConverterOption(false, "L");
            Map(m => m.PAON).Index(7);
            Map(m => m.SAON).Index(8);
            Map(m => m.Street).Index(9);
            Map(m => m.Locality).Index(10);
            Map(m => m.Town).Index(11);
            Map(m => m.District).Index(12);
            Map(m => m.County).Index(13);
            Map(m => m.StandardPrice).Index(14).TypeConverterOption(true, "A").TypeConverterOption(false, "B");
            Map(m => m.RecordStatus).Index(15);
        }
    }
}
