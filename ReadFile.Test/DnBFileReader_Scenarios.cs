using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ReadFile.Test
{
    public class DnBFileReader_Scenarios
    {
        [Fact]
        public void GetCompanies_Successful()
        {
            var path = @"DnbFiles\Scenario_TwoHeaders.txt";
            var dnbReader = new DnBFileReader(path);

            var companies = dnbReader.GetCompanies().ToList();

            Assert.Equal(companies.Count(), 3);

            var company1 = companies[0];
            var company2 = companies[1];
            var company3 = companies[2];

            Assert.Equal(company1.No, "00001000145");
            Assert.Equal(company2.No, "00001000152");
            Assert.Equal(company3.No, "00001000223");

            Assert.Equal(company1.Name, "Paradise Tan");
            Assert.Equal(company2.Name, "Cuddle Time");
            Assert.Equal(company3.Name, "Business Internet Print & Design");

            Assert.Equal(company1.Country, "USA");
            Assert.Equal(company2.Country, "USA");
            Assert.Equal(company3.Country, "USA");
        }
    }
}
