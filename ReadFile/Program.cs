using Neo4jClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ReadFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program will read the big text file using Reactive Extensions");

            var path = @"C:\DevTools\test.txt";
            //var path = @"C:\DevTools\shortTest.txt";

            //var reader = new DnBFileReader();
            //var entities = reader.ReadFile(path);

            //entities.Take(10).ToList().ForEach(entity =>
            //{
            //    Console.WriteLine($"\t{reader.ToString(entity)}");
            //});


            var comp1 = new Company { No = "001", Name = "Hugh", Country = "Switzerland" };
            var comp2 = new Company { No = "002", Name = "Astrid", Country = "Britain" };
            var comp3 = new Company { No = "003", Name = "Quynh", Country = "Vietnam" };
            var rel1 = new Relationship { ParentNo = "001", ChildNo = "002", RelType = RelationshipType.Domestic };
            var rel2 = new Relationship { ParentNo = "001", ChildNo = "003", RelType = RelationshipType.Global };

            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), username: "neo4j", password: "qwerty123");
            client.Connect();

            //var graph = new DnBGraph(client);
            //graph.Add(comp1).Wait();
            //graph.Add(comp2).Wait();
            //graph.Add(comp3).Wait();
            //graph.AddRelationship(rel1).Wait();
            //graph.AddRelationship(rel2).Wait();

            Console.ReadLine();
        }
    }

    public class DnBFileReader
    {
        private int GetNumberOfHeaderRows(string fileName)
        {
            const char chrSeparator = '|';
            const string headerTwoCols = "D-U-N-S NUMBER|BUSINESS NAME"; //The first two header columns

            using (var file = new StreamReader(fileName, Encoding.Default))
            {
                var count = 0;
                for (var i = 0; i < 5; i++)
                {
                    var fileHeaderRow = file.ReadLine();
                    if (fileHeaderRow == null) continue;

                    var fileHeaderArr = fileHeaderRow.Split(chrSeparator).Take(2);
                    var fileTwoHeaderCols = string.Join(chrSeparator.ToString(), fileHeaderArr);
                    if (string.Equals(fileTwoHeaderCols, headerTwoCols, StringComparison.InvariantCultureIgnoreCase))
                        count++;
                    else
                        return count;
                }
                return count;
            }
        }

        public IEnumerable<DnBEntity> ReadFile(string fileName)
        {
            var headerCount = GetNumberOfHeaderRows(fileName);

            using (var file = new StreamReader(fileName, Encoding.Default))
            {
                for (int i = 0; i < headerCount; i++)
                {
                    file.ReadLine(); //skip the header rows
                }

                while (true)
                {
                    var line = file.ReadLine();

                    if (line == null)
                        yield break;
                    else
                    {
                        var csv = line.Split('|');
                        yield return new DnBEntity
                        {
                            No = csv[0],
                            Name = csv[1],
                            Country = csv[8],
                            HeadquarterDuns = csv[62],
                            DomesticDuns = csv[74],
                            GuoDuns = csv[84]
                        };
                    }
                }
            }
        }

        public string ToString(DnBEntity entity)
        {
            return $"Duns: {entity.No}; Name: {entity.Name}; Country: {entity.Country}; Headquarter: {entity.HeadquarterDuns}; Domestic: {entity.DomesticDuns}; Global: {entity.GuoDuns}";
        }
    }

    public class DnBEntity
    {
        //Dnb header
        //d_u_n_s_number,business_name,tradestyle_name,rgstrd_address_ind,street_address,street_address_2,city_name,state_province_name,country_name,dnb_city_code,dnb_county_code,dnb_state_prvnc_code,state_prvmc_abbr,dnb_country_code,postal_code,dnb_continent_code,mailing_address,mailing_city_name,mailing_county_name,mailing_state_prvnc,mailing_country_name,dnb_mail_city_code,dnb_mail_county_code,dnb_ml_state_prv_cod,mail_state_prvnc_abb,mcntrycode,mpostalcd,mcontcode,natnl_id,natnlid_cd,cntryacccd,telephone,cabletelex,fax_number,ceo_name,ceo_title,line_busin,sic1,sic2,sic3,sic4,sic5,sic6,prlocactcd,activ_ind,year_start,localsales,salesind1,us_sales,curr_code,emp_here,emphereind,employees_total,employees_total_ind,principals_inc_ind,import_export_agent_co,legal_status_code,filler,status_code,subsidiary_indicator,previous_d_u_n_s_numb,full_report_date,headquarter_parent_d_u,hqparname,hqparaddr,hqparcity,hqparstprv,hqparcntry,hq_city_cd,hq_cnty_cd,hqstprvabr,hq_cntrycd,hqpostalcd,hq_cont_cd,dom_ult_duns,domult_name,domult_addr,domult_city,dult_stprv,dult_city_cd,du_cntry_cd,du_stprv_abr,du_postal_cd,ultimat_ind,gu_d_u_n_s_number,gu_business_name,gu_street_address,gu_city_name,gu_state_province,gu_country_name,gu_dnb_city_code,gu_dnb_county_code,gu_state_province1,gu_dnb_country_code,gu_postal_code,gu_dnb_continent_cd,no_of_family_members,dias_code,hierarchy_code,last_update_date,marketing_pre_screen,veteran_indicator,women_owned_indicator,minority_owned_indicator,minority_type,markeetability_indicator

        [JsonProperty(PropertyName = "no")]
        public string No { get; set; } //1

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } //2

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; } //9

        [JsonProperty(PropertyName = "headno")]
        public string HeadquarterDuns { get; set; } //63

        [JsonProperty(PropertyName = "domesticno")]
        public string DomesticDuns { get; set; } //75

        [JsonProperty(PropertyName = "globalno")]
        public string GuoDuns { get; set; } //85
    }

    public class Company
    {
        [JsonProperty(PropertyName = "no")]
        public string No { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }

    public class Relationship
    {
        public string ChildNo { get; set; }
        public string ParentNo { get; set; }
        public RelationshipType RelType { get; set; }
    }

    public enum RelationshipType
    {
        Headquarter,
        Domestic,
        Global
    }

    public class DnBGraph
    {
        private IGraphClient client;

        public DnBGraph(GraphClient client)
        {
            this.client = client;
        }

        public async Task AddCompany(Company company)
        {
            await client.Cypher.Create("(company:Company {newComp})")
                                .WithParam("newComp", company)
                                .ExecuteWithoutResultsAsync();
        }

        public async Task AddRelationship(Relationship relationship)
        {
            await client.Cypher.Match("(child:Company)", "(parent:Company)")
                                .Where((Company child) => child.No == relationship.ChildNo)
                                .AndWhere((Company parent) => parent.No == relationship.ParentNo)
                                .Create("child-[:BELONGS_TO { type: {r}}]->parent")
                                .WithParam("r", relationship.RelType)
                                .ExecuteWithoutResultsAsync();
        }
    }
}