using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using BenchmarkDotNet.Attributes;
using Dapper;
using RDapter;
//using RDapter.Extends;
using RDapter.Runner.Models;

namespace RDapter.Runner
{
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class Test
    {
        private const string ConnectionString = "Filename=:memory:";
        [Benchmark]
        public List<taxifaretrain> Dapper1()
        {
            using var connection = new SqliteConnection(ConnectionString);
            var res = connection.Query<taxifaretrain>("SELECT * FROM data").AsList();
            return res;

        }
        [Benchmark]
        public List<taxifaretrain> RDapter1()
        {
            using var connection = new SqliteConnection(ConnectionString);
            var res = connection.ExecuteReader<taxifaretrain>("SELECT * FROM data").AsList();
            return res;
        }
        [Benchmark]
        public List<taxifaretrain> RDapterExtends()
        {
            using var connection = new SqliteConnection(ConnectionString);
            var res = RDapter.Extends.Mapper.Query<taxifaretrain>(connection, top: 10000).AsList();
            return res;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            RDapter.Global.SetBuilderFor<taxifaretrain>((constraint) =>
            {
                constraint.Property(x => x.fare_amount).IsPrimaryKey().IsRequired().HasName("");
            });
            var constraint = RDapter.Global.GetSchemaConstraint<taxifaretrain>();
            var test = new Test();
            var res = test.RDapterExtends();
            //BenchmarkDotNet.Running.BenchmarkRunner.Run<Test>();
            return;
        }
    }
}
