using CsvHelper;
using CsvHelper.Configuration;
using Neo4jClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            //LoadGps(client);

            LoadProperties(client, @"Data\pp-2016.csv");
        }

        static void LoadProperties(GraphClient client, string filePath)
        {
            //Go through the whole file to make sure there is no problem
            // Any error will be thrown here - and the process will stop
            foreach (var item in new PropertyReader(filePath, false).Read()) { }

            //Read all existing postcodes in the graph database
            var x = client.Cypher.Match("(p:PostCode)").Return(p => p.As<PostCode>().Full).Results;
            var postcodes = new HashSet<string>(x);

            var y = client.Cypher.Match("(p:PostCodeInit)").Return(p => p.As<PostCodeInit>().Init).Results;
            var postcodeinits = new HashSet<string>(y);

            var propReader = new PropertyReader(filePath, false);
            var properties = propReader.Read();
            int i = 0;
            foreach (var property in properties)
            {
                i++;

                if (i > 1000)
                {
                    i = 0;
                    Console.WriteLine("\t1k properties added");
                }

                //Add a property node
                client.Cypher.Create("foo:Property {newProp}")
                        .WithParam("newProp", property)
                        .ExecuteWithoutResults();

                //Add PostCode + PostCodeInit nodes if they have not been seen before
                //PostCode
                if (string.IsNullOrWhiteSpace(property.PostCode)
                    && !postcodes.Contains(property.PostCode)) //new postcode, never seen before
                {
                    postcodes.Add(property.PostCode);

                    var postcode = new PostCode
                    {
                        Full = property.PostCode,
                        Init = property.PostCodeInit
                    };

                    //create a new PostCode
                    client.Cypher.Create("(foo:PostCode {postcode})")
                                    .WithParam("postcode", postcode)
                                    .ExecuteWithoutResults();
                }

                //PostCodeInit
                if (string.IsNullOrWhiteSpace(property.PostCodeInit)
                    && !postcodeinits.Contains(property.PostCodeInit)) //new postcodeinit, never seen before
                {
                    postcodeinits.Add(property.PostCodeInit);

                    var postcodeinit = new PostCodeInit
                    {
                        Init = property.PostCodeInit
                    };

                    //create a new PostCodeInit
                    client.Cypher.Create("(foo:PostCodeInit {p})")
                                    .WithParam("p", postcodeinit)
                                    .ExecuteWithoutResults();
                }

                //Create index on PostCode and PostCodeInit on Property nodes

            }
        }

        static void LoadGps(GraphClient client)
        {
            var gpPath = @"Data\GPs.csv";
            var gpReader = new GpReader(gpPath);
            var gps = gpReader.Read().ToList();

            //Trim relevant columns to avoid silly mistakes
            //foreach (var item in gps)
            //{
            //    item.NationalGrouping = item.NationalGrouping.Trim();
            //    item.HighLevelHealth = item.HighLevelHealth.Trim();
            //    item.PostCode = item.PostCode.Trim();
            //}

            Console.WriteLine("\tList of GPs loaded");

            //var tasks = gps.Select(item =>
            //{
            //    return client.Cypher.Create("(foo:GP {newGP})")
            //        .WithParam("newGP", item)
            //        .ExecuteWithoutResultsAsync();
            //}).ToList();
            //Task.WhenAll(tasks).Wait();

            foreach (var item in gps)
            {
                client.Cypher.Create("(foo:GP {newGP})")
                    .WithParam("newGP", item)
                    .ExecuteWithoutResults();
            }

            Console.WriteLine("\tList of GPs added into Neo4J");

            //National Grouping nodes
            var nationalGroupings = gps.Select(foo => foo.NationalGrouping)
                                        .Distinct()
                                        .Select(foo => new NationalGrouping
                                        {
                                            Name = foo
                                        })
                                        .ToList();

            foreach (var item in nationalGroupings)
            {
                //create a new grouping
                client.Cypher.Create("(foo:NationalGrouping {grouping})")
                                .WithParam("grouping", item)
                                .ExecuteWithoutResults();

                //create all relationships with GPs in the locations
                client.Cypher
                      .Match("(gp:GP)", "(grouping:NationalGrouping)")
                      .Where((GpRecord gp) => gp.NationalGrouping == item.Name)
                      .AndWhere((NationalGrouping grouping) => grouping.Name == item.Name)
                      .CreateUnique("gp-[:BELONGS_TO_GROUP]->grouping")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("\tNational Grouping nodes added");


            //High Level Health
            var highLevelHealths = gps.Select(foo => foo.HighLevelHealth)
                                        .Distinct()
                                        .Select(foo => new HighLevelHealth
                                        {
                                            Name = foo
                                        })
                                        .ToList();

            foreach (var item in highLevelHealths)
            {
                //create a new HighLevelHealth
                client.Cypher.Create("(foo:HighLevelHealth {health})")
                                .WithParam("health", item)
                                .ExecuteWithoutResults();

                //create all relationships with GPs in the same HighLevelHealth
                client.Cypher
                      .Match("(gp:GP)", "(health:HighLevelHealth)")
                      .Where((GpRecord gp) => gp.HighLevelHealth == item.Name)
                      .AndWhere((HighLevelHealth health) => health.Name == item.Name)
                      .CreateUnique("gp-[:BELONGS_TO_HEALTH]->health")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("\tHigh Level Health nodes added");

            //PostCodes
            var postCodes = gps.Select(foo => foo.PostCode)
                                .Where(foo => !string.IsNullOrWhiteSpace(foo))
                                .Distinct()
                                .Select(foo => new PostCode
                                {
                                    Init = foo.Split(' ').First(),
                                    Full = foo
                                })
                                .ToList();

            foreach (var item in postCodes)
            {
                //create a new PostCode
                client.Cypher.Create("(foo:PostCode {postcode})")
                                .WithParam("postcode", item)
                                .ExecuteWithoutResults();

                //create all relationships with GPs in the same PostCode
                client.Cypher
                      .Match("(gp:GP)", "(p:PostCode)")
                      .Where((GpRecord gp) => gp.PostCode == item.Full)
                      .AndWhere((PostCode p) => p.Full == item.Full)
                      .CreateUnique("gp-[:BELONGS_TO_POSTCODE]->p")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("\tPostCode nodes added");

            //PostCodeInits
            var postCodeInits = gps.Select(foo => foo.PostCodeInit)
                                    .Where(foo => !string.IsNullOrWhiteSpace(foo))
                                    .Distinct()
                                    .Select(foo => new PostCodeInit
                                    {
                                        Init = foo
                                    }).ToList();

            foreach (var item in postCodeInits)
            {
                //create a new PostCodeInit
                client.Cypher.Create("(foo:PostCodeInit {p})")
                                .WithParam("p", item)
                                .ExecuteWithoutResults();

                //create all relationships with GPs in the same PostCodeInit
                client.Cypher
                      .Match("(gp:GP)", "(p:PostCodeInit)")
                      .Where((GpRecord gp) => gp.PostCodeInit == item.Init)
                      .AndWhere((PostCodeInit p) => p.Init == item.Init)
                      .CreateUnique("gp-[:BELONGS_TO_POSTCODEINIT]->p")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("\tPostCodeInit nodes added");
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

        [JsonProperty(PropertyName = "postcodeinit")]
        public string PostCodeInit { get; set; }

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

    public class NationalGrouping
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class HighLevelHealth
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class PostCode
    {
        [JsonProperty(PropertyName = "init")]
        public string Init { get; set; }

        [JsonProperty(PropertyName = "full")]
        public string Full { get; set; }
    }

    public class PostCodeInit
    {
        [JsonProperty(PropertyName = "init")]
        public string Init { get; set; }
    }

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

    public class PropTrans
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "dateoftransfer")]
        public DateTime DateOfTransfer { get; set; }

        [JsonProperty(PropertyName = "postcode")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "postcodeinit")]
        public string PostCodeInit { get; set; }
        //D Detached
        //S Semi-Detached
        //T   Terraced
        //F   Flats
        //O   Other

        [JsonProperty(PropertyName = "proptype")]
        public char PropertyType { get; set; }

        [JsonProperty(PropertyName = "new")]
        public bool NewBuilding { get; set; }

        [JsonProperty(PropertyName = "freehold")]
        public bool Freehold { get; set; }

        [JsonProperty(PropertyName = "paon")]
        public string PAON { get; set; }

        [JsonProperty(PropertyName = "saon")]
        public string SAON { get; set; }

        [JsonProperty(PropertyName = "street")]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "locality")]
        public string Locality { get; set; }

        [JsonProperty(PropertyName = "town")]
        public string Town { get; set; }

        [JsonProperty(PropertyName = "district")]
        public string District { get; set; }

        [JsonProperty(PropertyName = "county")]
        public string County { get; set; }

        [JsonProperty(PropertyName = "stdprice")]
        public bool StandardPrice { get; set; }

        [JsonProperty(PropertyName = "recstatus")]
        public char RecordStatus { get; set; }
    }

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
