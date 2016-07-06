using Neo4jClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world");

            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), username: "neo4j", password: "qwerty123");
            client.Connect();

            //CipherQueries(client);

            InsertRosslynReportingLines(client);

            Console.ReadLine();
        }

        static void CipherQueries(GraphClient client)
        {
            //Get all movies from the database
            var allMovies = client.Cypher
                                  .Match("(x:Movie)")
                                  .Return(x => x.As<Movie>())
                                  .Results;

            //NOTE: Must use the same variable (x) in both Match and Return clauses

            Console.WriteLine("\nThere is a total of {0} movies in the database", allMovies.Count());
            allMovies.OrderBy(foo => foo.Title).ToList().ForEach(movie =>
            {
                Console.WriteLine($"\t{movie.Title} - {movie.Released}");
            });

            //Get movies by labels
            Console.WriteLine("\nTop 10 movies:");
            client.Cypher
                  .Match("(m:Movie)")
                  .Return(m => m.As<Movie>())
                  .Limit(10)
                  .Results
                  .ToList()
                  .ForEach(movie =>
                  {
                      Console.WriteLine("{0} ({1}) - {2}", movie.Title, movie.Released, movie.TagLine);
                  });

            //Get movies released in 2000
            Console.WriteLine("\nMovies released in 2000:");
            client.Cypher
                .Match("(m:Movie)")
                .Where((Movie m) => m.Released == 2000)
                .Return(m => m.As<Movie>())
                .Results
                .ToList()
                .ForEach(movie =>
                {
                    Console.WriteLine("{0} ({1}) - {2}", movie.Title, movie.Released, movie.TagLine);
                });

            //Get Tom Hanks' movies
            Console.WriteLine("\nMovies by Tom Hanks:");
            client.Cypher
                //.OptionalMatch("(a:Person)-[ACTED_IN]-(m:Movie)")
                .Match("(a:Person)-[:ACTED_IN]-(m:Movie)")
                .Where((Person a) => a.Name == "Tom Hanks")
                .Return((a, m) => new
                {
                    Actor = a.As<Person>(),
                    Movie = m.As<Movie>()
                })
                .Results
                .OrderBy(foo => foo.Movie.Released)
                .ToList()
                .ForEach(obj =>
                {
                    Console.WriteLine("{0} ({1}) - {2}", obj.Movie.Title, obj.Movie.Released, obj.Movie.TagLine);
                });

            //(TomH) -[:ACTED_IN { roles:['Jim Lovell']}]->(Apollo13),
        }

        static void InsertRosslynReportingLines(GraphClient client)
        {
            //Wipe out all the existing employees and their relationship
            //TODO
            //client.Cypher.OptionalMatch("")

            //List of employees to be added
            var employees = new List<Employee>
            {
                new Employee { Id = 0, Name = "Hugh"},
                new Employee { Id = 1, Name = "Quynh"},
                new Employee { Id = 11, Name = "Rodrigo"},
                new Employee { Id = 111, Name = "Michael"},
                new Employee { Id = 112, Name = "Cuong"},
                new Employee { Id = 113, Name = "Adam"},
                new Employee { Id = 12, Name = "Aman"},
                new Employee { Id = 121, Name = "Sergio"},
                new Employee { Id = 122, Name = "Harmeet"},
                new Employee { Id = 123, Name = "Burak"},
                new Employee { Id = 124, Name = "Jagath"},
                new Employee { Id = 125, Name = "Sergii"},
                new Employee { Id = 13, Name = "Wesley"},
                new Employee { Id = 2, Name = "Astrid"},
                new Employee { Id = 21, Name = "Richard"},
                new Employee { Id = 22, Name = "Jacob"},
                new Employee { Id = 23, Name = "Laben"},
                new Employee { Id = 24, Name = "Ambar"},
            };

            foreach (var emp in employees)
            {
                //upsert the node
                client.Cypher
                    .Merge("(employee:Employee { Id: {id} })")
                    .OnCreate()
                    .Set("employee = {e}")
                    .WithParams(new
                    {
                        id = emp.Id,
                        e = emp
                    })
                    .ExecuteWithoutResults();

                //not God -> must have a manager
                if (emp.Id > 0)
                {
                    var parentId = emp.Id / 10;
                    client.Cypher
                        .Match("(e:Employee)", "(m:Employee)")
                        .Where((Employee e) => e.Id == emp.Id)
                        .AndWhere((Employee m) => m.Id == parentId)
                        .CreateUnique("e-[:REPORTS_TO]->m")
                        .ExecuteWithoutResults();
                }
            }

            Console.WriteLine("Rosslyn reporting lines added");
        }
    }

    public class Movie
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "released")]
        public int Released { get; set; }

        [JsonProperty(PropertyName = "tagline")]
        public string TagLine { get; set; }
    }

    public class Person
    {
        [JsonProperty(PropertyName = "born")]
        public int Born { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class Employee
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
