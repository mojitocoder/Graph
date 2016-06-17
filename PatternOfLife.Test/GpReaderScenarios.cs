using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PatternOfLife.Test
{
    public class GpReaderScenarios
    {
        [Fact]
        public void GetCompanies_Successful()
        {
            var path = @"Data\GPsSample.csv";
            var gpReader = new GpReader(path);
            var gps = gpReader.Read().ToList();

            Assert.Equal(gps.Count, 10);
            Assert.Equal(gps.Select(foo => foo.Code), new List<string> { "G0102005", "G0102926", "G0105912", "G0107031", "G0107725", "G0108018", "G0108324", "G0108867", "G0108922", "G0109325" });
            Assert.Equal(gps.Select(foo => foo.Name), new List<string> { "ALLEN EB", "ANDERSON MG", "ADLER S", "ATTWOOD DC", "ALEXANDER PJ", "ALLDRIDGE DGE", "ANDERSON CF", "ANTHONY RAJ", "ASHER PN", "ARTHUR RA" });
            Assert.Equal(gps.Select(foo => foo.NationalGrouping), new List<string> { "Y11", "Y55", "Y56", "Y54", "Y01", "Y57", "Y54", "Y57", "Y56", "Y56" });
            Assert.Equal(gps.Select(foo => foo.HighLevelHealth), new List<string> { "QAL", "Q79", "Q71", "Q83", "QDF", "Q81", "Q74", "Q82", "Q71", "Q71" });
            Assert.Equal(gps.Select(foo => foo.Address1), new List<string> { "FIRCROFT, LONDON ROAD", "LENSFIELD MEDICAL PRAC.", "682 FINCHLEY ROAD", "GREAT LEVER HEALTH CENTRE", "10 WEST END", "OAKFIELD", "THE HEALTH CENTRE", "THE LECKHAMPTON SURGERY", "94-96 HOLLOWAY ROAD", "153 CANNON HILL LANE" });
            Assert.Equal(gps.Select(foo => foo.Address2), new List<string> { "ENGLEFIELD GREEN", "48 LENSFIELD ROAD", "GOLDERS GREEN", "RUPERT STREET,GREAT LEVER", "SWANLAND", "158 STATION ROAD", "LAWSON STREET", "LLOYD DAVIES HOUSE", "", "RAYNES PARK" });
            Assert.Equal(gps.Select(foo => foo.Address3), new List<string> { "EGHAM", "CAMBRIDGE", "LONDON", "BOLTON", "HUMBERSIDE", "REDHILL", "STOCKTON ON TEES", "17 MOOREND PARK ROAD", "LONDON", "WEST WIMBLEDON" });
            Assert.Equal(gps.Select(foo => foo.Address4), new List<string> { "SURREY", "", "", "LANCASHIRE", "", "SURREY", "CLEVELAND", "CHELTENHAM", "", "LONDON" });
            Assert.Equal(gps.Select(foo => foo.Address5), new List<string> { "", "", "", "", "", "", "", "", "", "xx" });
            Assert.Equal(gps.Select(foo => foo.PostCode), new List<string> { "TW20 0BS", "CB2 1EH", "NW11 7NP", "BL3 6RN", "HU14 3PE", "RH1 1HF", "TS18 1HU", "GL53 0LA", "N7 8JG", "SW20 9DA" });
            Assert.Equal(gps.Select(foo => foo.StatusCode), new List<char> { 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A' });
            Assert.Equal(gps.Select(foo => foo.TypeCode), new List<char> { 'P', 'O', 'O', 'O', 'P', 'O', 'O', 'O', 'O', 'O' });
            Assert.Equal(gps.Select(foo => foo.ParentCode), new List<string> { "H81600", "D81001", "E83600", "P82013", "B81600", "H81083", "A81001", "L84040", "F83013", "H85016" });
            Assert.Equal(gps.Select(foo => foo.ContactNo), new List<string> { "", "0844 3878222", "020 84559994", "01204 462141", "0482 633570", "01737 642207", "01642 672351", "0242 515363", "1716072323", "020 85425201" });
            Assert.Equal(gps.Select(foo => foo.Amended), new List<bool> { false, false, false, true, false, false, false, false, false, false });
            Assert.Equal(gps.Select(foo => foo.CurrentCareOrg), new List<string> { "", "06H", "07M", "00T", "", "09L", "00K", "11M", "08H", "08R" });
            Assert.Equal(gps.Select(foo => foo.OpenDate), new List<string> { "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401" }.Select(foo => foo.ToDate()));
            Assert.Equal(gps.Select(foo => foo.CloseDate), new List<string> { "", "", "", "", "", "", "", "", "", "" }.Select(foo => foo.ToDate()));
            Assert.Equal(gps.Select(foo => foo.JoinParentDate), new List<string> { "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401", "19740401" }.Select(foo => foo.ToDate()));
            Assert.Equal(gps.Select(foo => foo.LeftParentDate), new List<string> { "19910401", "19911231", "19920731", "19910525", "19940409", "19920102", "19910701", "19930427", "19951102", "19920501" }.Select(foo => foo.ToDate()));
        }
    }

    public static class Helper
    {
        public static DateTime? ToDate(this string date)
        {
            if (string.IsNullOrEmpty(date)) return null;
            else
                return new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));
        }
    }
}
