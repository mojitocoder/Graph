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

            //LoadProperties(client, @"Data\pp-2016.csv");


        }

        static void LoadProperties(GraphClient client, string filePath)
        {
            Console.WriteLine("Property file starts being loaded: {0}", filePath);

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
                client.Cypher.Create("(foo:Property {newProp})")
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

        //AIzaSyBiEgORpbZqJP0tsn6bQSeQJRtrYgvtHUc
    }

    public class Crime
    {
        public string Id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public string ReportedBy { get; set; }

        public string FallsWithin { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Location { get; set; }

        //LSOA: Layer Super Output Area
        public string LsoaCode { get; set; }

        public string LsoaName { get; set; }

        public string Type { get; set; }

        public string OutcomeCategory { get; set; }

        public string Context { get; set; }
    }

    public sealed class CrimeMap : CsvClassMap<Crime>
    {
        public CrimeMap()
        {
            Map(m => m.Id).Name("Crime ID");
            Map(m => m.Month).ConvertUsing(row => int.Parse(row.GetField<string>("Month").Trim().Split('-')[1]));
            Map(m => m.Year).ConvertUsing(row => int.Parse(row.GetField<string>("Month").Trim().Split('-')[0]));
            Map(m => m.ReportedBy).Name("Reported by");
            Map(m => m.FallsWithin).Name("Falls within");
            Map(m => m.Longitude).Name("Longitude");
            Map(m => m.Latitude).Name("Latitude");
            Map(m => m.Location).Name("Location");
            Map(m => m.LsoaCode).Name("LSOA code");
            Map(m => m.LsoaName).Name("LSOA name");
            Map(m => m.Type).Name("Crime type");
            Map(m => m.OutcomeCategory).Name("Last outcome category");
            Map(m => m.Context).Name("Context");
        }
    }

    public class CrimeReader
    {
        private string filePath;

        public CrimeReader(string filePath)
        {
            this.filePath = filePath;
        }

        public IEnumerable<Crime> Read()
        {
            using (var file = new StreamReader(filePath))
            {
                var csv = new CsvReader(file);
                csv.Configuration.RegisterClassMap<CrimeMap>();
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

                    yield return csv.GetRecord<Crime>();
                }
            }
        }
    }


}
