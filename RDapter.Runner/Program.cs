using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        [Benchmark]
        public List<taxifaretrain> Dapper1()
        {
            using var connection = new SqlConnection("server=localhost;database=local;user=sa;password=sa;");
            var res = connection.Query<taxifaretrain>("SELECT TOP(10000) * FROM [taxi-fare-train]").AsList();
            return res;

        }
        [Benchmark]
        public List<taxifaretrain> RDapter1()
        {
            using var connection = new SqlConnection("server=localhost;database=local;user=sa;password=sa;");
            var res = connection.ExecuteReader<taxifaretrain>("SELECT TOP(10000) * FROM [taxi-fare-train]").AsList();
            return res;
        }
        [Benchmark]
        public List<taxifaretrain> RDapterExtends()
        {
            using var connection = new SqlConnection("server=localhost;database=local;user=sa;password=sa;");
            var res = RDapter.Extends.Mapper.Query<taxifaretrain>(connection, top: 10000).AsList();
            return res;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            RDapter.Global.SetSchemaConstraint<taxifaretrain>((constraint) =>
            {
                //constraint.SetField(nameof(taxifaretrain.vendorid), "vendor_id", true, true);
                //constraint.SetTableName("[taxi-fare-train]");
                constraint.SetField<taxifaretrain>(x => x.vendorid, "vendor_id");
            });
            var constraint = RDapter.Global.GetSchemaConstraint<taxifaretrain>();
            var test = new Test();
            var res = test.RDapterExtends();
            //BenchmarkDotNet.Running.BenchmarkRunner.Run<Test>();
            return;
        }
    }
}
