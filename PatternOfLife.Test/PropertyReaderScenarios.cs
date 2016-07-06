using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PatternOfLife.Test
{
    public class PropertyReaderScenarios
    {
        [Fact]
        public void GetProperties_Successful()
        {
            var path = @"Data\props.csv";
            var propReader = new PropertyReader(path, false);

            var props = propReader.Read().ToList();

            Assert.True(true);

            Assert.Equal(props.Count, 5);
            Assert.Equal(props.Select(foo => foo.Id), new List<string> { "{2FD36065-378C-4BF8-E050-A8C0620562B1}", "{2FD36065-378E-4BF8-E050-A8C0620562B1}", "{2FD36065-378F-4BF8-E050-A8C0620562B1}", "{2FD36065-3792-4BF8-E050-A8C0620562B1}", "{2FD36065-3793-4BF8-E050-A8C0620562B1}" });
            Assert.Equal(props.Select(foo => foo.Price), new List<int> { 523000, 270000, 535000, 250000, 429995 });
            Assert.Equal(props.Select(foo => foo.DateOfTransfer), new List<DateTime> { new DateTime(2016, 1, 8), new DateTime(2016, 2, 1), new DateTime(2016, 1, 22), new DateTime(2016, 1, 5), new DateTime(2016, 1, 27) });
            Assert.Equal(props.Select(foo => foo.PostCode), new List<string> { "SL9 0EQ", "SL2 3NW", "SL9 0ED", "HP11 1JB", "MK7 6JQ" });
            Assert.Equal(props.Select(foo => foo.PostCodeInit), new List<string> { "SL9", "SL2", "SL9", "HP11", "MK7" });
            Assert.Equal(props.Select(foo => foo.PropertyType), new List<char> { 'F', 'F', 'F', 'F', 'D' });
            Assert.Equal(props.Select(foo => foo.NewBuilding), new List<bool> { true, false, true, false, true });
            Assert.Equal(props.Select(foo => foo.Freehold), new List<bool> { false, false, false, true, false });
            Assert.Equal(props.Select(foo => foo.PAON), new List<string> { "PORTLAND COURT", "READE COURT", "6", "172A", "15" });
            Assert.Equal(props.Select(foo => foo.SAON), new List<string> { "4", "FLAT 2", "", "FLAT 5", "" });
            Assert.Equal(props.Select(foo => foo.Street), new List<string> { "CHALFONT DENE", "VICTORIA ROAD", "MILTON PLACE", "KINGSMEAD ROAD", "BEDGEBURY PLACE" });
            Assert.Equal(props.Select(foo => foo.Locality), new List<string> { "CHALFONT ST PETER", "FARNHAM COMMON", "CHALFONT ST PETER", "", "KENTS HILL" });
            Assert.Equal(props.Select(foo => foo.Town), new List<string> { "GERRARDS CROSS", "SLOUGH", "GERRARDS CROSS", "HIGH WYCOMBE", "MILTON KEYNES" });
            Assert.Equal(props.Select(foo => foo.District), new List<string> { "CHILTERN", "SOUTH BUCKS", "CHILTERN", "WYCOMBE", "MILTON KEYNES" });
            Assert.Equal(props.Select(foo => foo.County), new List<string> { "BUCKINGHAMSHIRE", "BUCKINGHAMSHIRE", "BUCKINGHAMSHIRE", "BUCKINGHAMSHIRE", "MILTON KEYNES" });
            Assert.Equal(props.Select(foo => foo.StandardPrice), new List<bool> { true, true, true, true, false });
            Assert.Equal(props.Select(foo => foo.RecordStatus), new List<char> { 'A', 'A', 'A', 'A', 'D' });
        }
    }
}
