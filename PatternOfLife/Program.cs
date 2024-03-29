﻿using CsvHelper;
using CsvHelper.Configuration;
using Neo4jClient;
using Newtonsoft.Json;
using PatternOfLife.GeoCoding;
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

            //GeoCrimeFiles(); //Reverse geo-code the crime data files

            //LoadGeoedCrimeFiles(client);

            //CreateLinkFromPostcodeToPropertiesAndCrimes(client);

            Console.ReadLine();
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

        static void GeoCrimeFiles()
        {
            var folder = @"Data\Crime";
            const string apiKey = "AIzaSyBiEgORpbZqJP0tsn6bQSeQJRtrYgvtHUc"; //test API key

            var crimeGeo = new CrimeReverserGeoCode(folder, apiKey);
            crimeGeo.Start();
        }

        static void LoadGeoedCrimeFiles(GraphClient client)
        {
            var folderPath = @"Data\Crime";

            var files = Directory.GetFiles(folderPath, "*.csv").Where(foo => Path.GetFileNameWithoutExtension(foo).Last() == '_').ToList();

            var crimes = files.SelectMany(file => new GeoedCrimeReader(file).Read().ToList())
                                .Where(foo => foo.PostCodeInit != "")
                                .ToList();

            var postcodeCrimeCount = crimes.GroupBy(foo => foo.PostCodeInit, foo => foo)
                            .Select(foo => new
                            {
                                PostCodeInit = foo.Key,
                                Count = foo.Count()
                            })
                            .OrderByDescending(foo => foo.Count)
                            .ToList();

            foreach (var item in postcodeCrimeCount)
            {
                Console.WriteLine($"{item.PostCodeInit} :{item.Count}");
            }

            //Load crimes into the graph
            foreach (var item in crimes)
            {
                client.Cypher.Create("(foo:Crime {newCrime})")
                    .WithParam("newCrime", item)
                    .ExecuteWithoutResults();
            }

            Console.WriteLine("Crime data loaded");
        }

        static void CreateLinkFromPostcodeToPropertiesAndCrimes(GraphClient client)
        {
            //Read all existing postcodes in the graph database
            var postcodes = client.Cypher.Match("(p:PostCode)").Return(p => p.As<PostCode>()).Results.ToList();
            var postcodeInits = client.Cypher.Match("(p:PostCodeInit)").Return(p => p.As<PostCodeInit>()).Results.ToList();

            foreach (var item in postcodes)
            {
                client.Cypher
                      .Match("(prop:Property)", "(p:PostCode)")
                      .Where((PropTrans prop) => prop.PostCode == item.Full)
                      .AndWhere((PostCode p) => p.Full == item.Full)
                      .CreateUnique("prop-[:EXCHANGED_IN_POSTCODE]->p")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("Done Prop - PostCode");

            foreach (var item in postcodeInits)
            {
                client.Cypher
                      .Match("(prop:Property)", "(p:PostCodeInit)")
                      .Where((PropTrans prop) => prop.PostCodeInit == item.Init)
                      .AndWhere((PostCodeInit p) => p.Init == item.Init)
                      .CreateUnique("prop-[:EXCHANGED_IN_POSTCODEINIT]->p")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("Done Prop - PostCodeInit");

            foreach (var item in postcodes)
            {
                client.Cypher
                      .Match("(c:Crime)", "(p:PostCode)")
                      .Where((Crime c) => c.PostCode == item.Full)
                      .AndWhere((PostCode p) => p.Full == item.Full)
                      .CreateUnique("c-[:HAPPENED_IN_POSTCODE]->p")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("Done Crime - PostCode");

            foreach (var item in postcodeInits)
            {
                client.Cypher
                      .Match("(c:Crime)", "(p:PostCodeInit)")
                      .Where((Crime c) => c.PostCodeInit == item.Init)
                      .AndWhere((PostCodeInit p) => p.Init == item.Init)
                      .CreateUnique("c-[:HAPPENED_IN_POSTCODEINIT]->p")
                      .ExecuteWithoutResults();
            }

            Console.WriteLine("Done Crime - PostCodeInit");
        }
    }

    public sealed class GeoedCrimeMap : CsvClassMap<Crime>
    {
        public GeoedCrimeMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.Month).Index(1);
            Map(m => m.Year).Index(2);
            Map(m => m.ReportedBy).Index(3);
            Map(m => m.FallsWithin).Index(4);
            Map(m => m.Longitude).Index(5);
            Map(m => m.Latitude).Index(6);
            Map(m => m.Location).Index(7);
            Map(m => m.LsoaCode).Index(8);
            Map(m => m.LsoaName).Index(9);
            Map(m => m.Type).Index(10);
            Map(m => m.OutcomeCategory).Index(11);
            Map(m => m.Context).Index(12);
            Map(m => m.PostCode).Index(13);
            Map(m => m.PostCodeInit).Index(14);
            Map(m => m.FormattedAddress).Index(15);
        }
    }


    public class CrimeReverserGeoCode
    {
        private string folder;
        private string apiKey;

        public CrimeReverserGeoCode(string folder, string apiKey)
        {
            this.folder = folder;
            this.apiKey = apiKey;
        }

        public IEnumerable<string> GetNonGeoFiles()
        {
            //any file starting with _ is done => no need to reverse geocode them
            //any file starting with a normal character need to be reverse geocoded
            var files = Directory.GetFiles(folder, "*.csv");

            var originalFiles = files.Select(foo => Path.GetFileNameWithoutExtension(foo))
                                        .Where(foo => foo.Last() != '_')
                                        .ToList();
            var geoedFiles = new HashSet<string>(files.Select(foo => Path.GetFileNameWithoutExtension(foo)).Where(foo => foo.Last() == '_').Select(foo => foo.Substring(0, foo.Length - 1)));

            return originalFiles.Where(foo => !geoedFiles.Contains(foo)).Select(foo => $"{foo}.csv").ToList();
        }

        public void GeoCrimeFile(string fileName)
        {
            var path = Path.Combine(this.folder, fileName);

            //read the file into memory
            var crimeReader = new CrimeReader(path);
            var crimes = crimeReader.Read();

            var targetFile = $"{Path.GetFileNameWithoutExtension(fileName)}_{Path.GetExtension(fileName)}";
            var targetPath = Path.Combine(this.folder, targetFile);

            Console.WriteLine("Start file: {0}", path);
            using (var writer = new StreamWriter(targetPath, false))
            {
                var csv = new CsvWriter(writer);
                var googleGeo = new GoogleGeo(apiKey);

                int i = 0;
                foreach (var item in crimes)
                {
                    i++;
                    if (i % 100 == 0) Console.WriteLine("\t100 records");

                    var result = googleGeo.ReverseGeoCode(item.Latitude, item.Longitude).Result;
                    var addressDetail = GoogleGeo.GetAddressDetail(result);

                    if (addressDetail != null)
                    {
                        item.FormattedAddress = addressDetail.FullAddress;
                        item.PostCode = addressDetail.PostCode;
                        item.PostCodeInit = addressDetail.PostCodeInit;
                    }

                    csv.WriteRecord(item);
                }
            }
        }

        public void Start()
        {
            var nonGeoFiles = GetNonGeoFiles();

            foreach (var item in nonGeoFiles)
            {
                GeoCrimeFile(item);
            }
        }
    }

    public class GeoedCrimeReader
    {
        private string filePath;

        public GeoedCrimeReader(string filePath)
        {
            this.filePath = filePath;
        }

        public IEnumerable<Crime> Read()
        {
            using (var file = new StreamReader(filePath))
            {
                var csv = new CsvReader(file);
                csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.RegisterClassMap<GeoedCrimeMap>();
                while (csv.Read())
                {
                    yield return csv.GetRecord<Crime>();
                }
            }
        }
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

        public string PostCode { get; set; }

        public string PostCodeInit { get; set; }

        public string FormattedAddress { get; set; }
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
            //Map(m => m.PostCode).Name("PostCode");
            //Map(m => m.PostCodeInit).Name("PostCodeInit");
            //Map(m => m.FormattedAddress).Name("FormattedAddress");
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
                    yield return csv.GetRecord<Crime>();
                }
            }
        }
    }
}
