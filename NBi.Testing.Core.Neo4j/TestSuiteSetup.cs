using Neo4j.Driver.V1;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Testing.Core.CosmosDb
{
    [SetUpFixture]
    public class TestSuiteSetup
    {
        [SetUp]
        public void Init()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.LoadMovies.cypher";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                
                var credentials = AuthTokens.Basic("neo4j", "bolt");
                var driver = GraphDatabase.Driver("bolt://127.0.0.1:7687", credentials);

                using (var session = driver.Session())
                {
                    var stopWatch = new Stopwatch();
                    Console.WriteLine("Truncating database ...");
                    stopWatch.Start();
                    var resDelete = session.WriteTransaction(tx => tx.Run("MATCH(n) OPTIONAL MATCH(n)-[r] - () WITH n, r LIMIT 1000 DELETE n, r"));
                    Console.WriteLine($"  {resDelete.Summary.Counters.NodesDeleted} nodes deleted.");
                    Console.WriteLine($"  {resDelete.Summary.Counters.RelationshipsDeleted} relationships deleted.");
                    Console.WriteLine($"Database truncated in {stopWatch.Elapsed.Milliseconds} milliseconds.");

                    stopWatch.Restart();
                    Console.WriteLine("Loading database with Movies ...");
                    while (!reader.EndOfStream)
                    {
                        var statement = reader.ReadToEnd();
                        var res = session.WriteTransaction(tx => tx.Run(statement));
                        Console.WriteLine($"  {res.Summary.Counters.NodesCreated} nodes created.");
                        Console.WriteLine($"  {res.Summary.Counters.RelationshipsCreated} relationships created.");
                    }
                    Console.WriteLine($"Database loaded with Movies in {stopWatch.Elapsed.Milliseconds} milliseconds.");
                }
                
            }
        }
    }
}