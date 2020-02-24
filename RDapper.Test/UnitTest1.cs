using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using RDapter;
using RDapter.Extends;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data.SQLite;

namespace RDapter.Test
{
    [TestFixture]
    internal class SQL
    {
        public class TestTable
        {
            public int id { get; set; }

            public string value { get; set; }
        }
        private string _msSqlConnection;
        private string _mySqlConnection;
        private string _sqliteConnection;

        [SetUp]
        public void Setup()
        {
            _msSqlConnection = @"Server=localhost;Database=Local;user=sa;password=sa;";
            _mySqlConnection = @"Server=localhost;Database=Local;Uid=root;Pwd=;";
            _sqliteConnection = $@"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Files\Local.db")};Version=3;";
            RDapter.Global.SetSchemaConstraint<TestTable>(constraint =>
            {
                constraint.SetTableName("TestTable");
                constraint.SetPrimaryKey("id", false);
            });
        }

        [Test]
        public void SQLServerConnectorCRUD()
        {
            using (var connection = new SqlConnection(_msSqlConnection))
            {
                try
                {
                    var datas = new List<TestTable>();
                    connection.CreateTable<TestTable>();
                    for (var iter = 0; iter < 10; iter++)
                    {
                        TestTable testTable = new TestTable() { id = iter, value = $"test" };
                        datas.Add(testTable);
                        var affectedCreate = connection.Insert(testTable);
                        Assert.AreEqual(affectedCreate, 1);
                        var selectedById = connection.Query<TestTable>(primaryKey: iter);
                        Assert.AreEqual(selectedById.id, iter);
                        Assert.AreEqual(selectedById.value, "test");
                        var selectedByIdLambda = connection.Query<TestTable>(x => x.id == iter).First();
                        Assert.AreEqual(selectedByIdLambda.id, iter);
                        Assert.AreEqual(selectedByIdLambda.value, "test");
                        var selectedAll = connection.Query<TestTable>().First();
                        Assert.AreEqual(selectedAll.id, iter);
                        Assert.AreEqual(selectedAll.value, "test");
                        var selectFirst = connection.QueryFirst<TestTable>();
                        Assert.AreEqual(selectFirst.id, iter);
                        Assert.AreEqual(selectFirst.value, "test");
                        testTable.value = null;//"updated";
                        var affectedUpdate = connection.Update(testTable);
                        Assert.AreEqual(affectedUpdate, 1);
                        var affectedDeleteLambdaInvalid = connection.Delete<TestTable>(x => x.id == iter + 1);
                        Assert.AreEqual(affectedDeleteLambdaInvalid, 0);
                        var affectedDelete = connection.Delete(testTable);
                        Assert.AreEqual(affectedDelete, 1);
                    }
                    var affectedSelectScalar = connection.Query<TestTable>();
                    Assert.AreEqual(affectedSelectScalar.Count(), 0);

                    Assert.AreEqual(datas.Count(), connection.InsertMany(datas));
                    Assert.Pass();
                }
                finally
                {
                    connection.ExecuteNonQuery("DROP TABLE [TestTable]");
                }
            }
        }

        [Test]
        public void SQLServerConnector()
        {
            using (var connection = new SqlConnection(_msSqlConnection))
            {
                try
                {
                    connection.ExecuteNonQuery($@"CREATE TABLE [dbo].[TestTable]([id] int primary key,[value] nvarchar(255))");
                    for (var iter = 0; iter < 10; iter++)
                    {
                        var affectedCreate = connection.ExecuteNonQuery(@"INSERT INTO [dbo].[TestTable]([id],[value]) VALUES(@id,@value)", new { id = iter, value = "test" });
                        Assert.AreEqual(affectedCreate, 1);
                        var selectedById = connection.ExecuteReader<TestTable>(@"SELECT * FROM [dbo].[TestTable] WHERE [id] = @id", new { id = iter }).First();
                        Assert.AreEqual(selectedById.id, iter);
                        Assert.AreEqual(selectedById.value, "test");
                        var selectedByIdDynamic = connection.ExecuteReader(@"SELECT * FROM [dbo].[TestTable] WHERE [id] = @id", new { id = iter }).First();
                        Assert.AreEqual(selectedByIdDynamic.id, iter);
                        Assert.AreEqual(selectedByIdDynamic.value, "test");
                        var affectedUpdate = connection.ExecuteNonQuery(@"UPDATE [dbo].[TestTable] SET [value] = @value WHERE [id] = @id", new { id = iter, value = "updated" });
                        Assert.AreEqual(affectedUpdate, 1);
                        var affectedDelete = connection.ExecuteNonQuery(@"DELETE FROM [dbo].[TestTable] WHERE [id] = @id", new { id = iter });
                        Assert.AreEqual(affectedDelete, 1);
                    }
                    var affectedSelectScalar = connection.ExecuteScalar<int>(@"SELECT COUNT(1) FROM [dbo].[TestTable]");
                    Assert.AreEqual(affectedSelectScalar, 0);
                    Assert.Pass();
                }
                finally
                {
                    connection.ExecuteNonQuery($@"DROP TABLE [dbo].[TestTable]");
                }
            }
        }

        public void MySQLConnector()
        {
            using (var connection = new MySqlConnection(_mySqlConnection))
            {
                try
                {
                    connection.ExecuteNonQuery($@"CREATE TABLE TestTable(id int primary key,value nvarchar(255))");
                    for (var iter = 0; iter < 10; iter++)
                    {
                        var affectedCreate = connection.ExecuteNonQuery(@"INSERT INTO TestTable(id,value) VALUES(@id,@value)", new { id = iter, value = "test" });
                        Assert.AreEqual(affectedCreate, 1);
                        var selectedById = connection.ExecuteReader<TestTable>(@"SELECT * FROM TestTable WHERE id = @id", new { id = iter }).First();
                        Assert.AreEqual(selectedById.id, iter);
                        Assert.AreEqual(selectedById.value, "test");
                        var selectedByIdDynamic = connection.ExecuteReader(@"SELECT * FROM TestTable WHERE id = @id", new { id = iter }).First();
                        Assert.AreEqual(selectedByIdDynamic.id, iter);
                        Assert.AreEqual(selectedByIdDynamic.value, "test");
                        var affectedUpdate = connection.ExecuteNonQuery(@"UPDATE TestTable SET value = @value WHERE id = @id", new { id = iter, value = "updated" });
                        Assert.AreEqual(affectedUpdate, 1);
                        var affectedDelete = connection.ExecuteNonQuery(@"DELETE FROM TestTable WHERE id = @id", new { id = iter, value = "test" });
                        Assert.AreEqual(affectedDelete, 1);
                    }
                    var affectedSelectScalar = connection.ExecuteScalar<long>(@"SELECT COUNT(1) FROM TestTable");
                    Assert.AreEqual(affectedSelectScalar, 0);
                    Assert.Pass();
                }
                finally
                {
                    connection.ExecuteNonQuery($@"DROP TABLE TestTable");
                }
            }
        }

        public void SQLiteConnector()
        {
            using (var connection = new SQLiteConnection(_sqliteConnection))
            {
                try
                {
                    connection.ExecuteNonQuery($@"CREATE TABLE TestTable(id int primary key,value nvarchar(255))");
                    for (var iter = 0; iter < 10; iter++)
                    {
                        var affectedCreate = connection.ExecuteNonQuery(@"INSERT INTO TestTable(id,value) VALUES(@id,@value)", new { id = iter, value = "test" });
                        Assert.AreEqual(affectedCreate, 1);
                        var selectedById = connection.ExecuteReader<TestTable>(@"SELECT * FROM TestTable WHERE id = @id", new { id = iter });
                        var actual = selectedById.First();
                        Assert.AreEqual(actual.id, iter);
                        Assert.AreEqual(actual.value, "test");
                        var selectedByIdDynamic = connection.ExecuteReader(@"SELECT * FROM TestTable WHERE id = @id", new { id = iter });
                        var dynamicActual = selectedByIdDynamic.First();
                        Assert.AreEqual(dynamicActual.id, iter);
                        Assert.AreEqual(dynamicActual.value, "test");
                        var affectedUpdate = connection.ExecuteNonQuery(@"UPDATE TestTable SET value = @value WHERE id = @id", new { id = iter, value = "updated" });
                        Assert.AreEqual(affectedUpdate, 1);
                        var affectedDelete = connection.ExecuteNonQuery(@"DELETE FROM TestTable WHERE id = @id", new { id = iter, });
                        Assert.AreEqual(affectedDelete, 1);
                    }
                    var affectedSelectScalar = connection.ExecuteScalar<long>(@"SELECT COUNT(1) FROM TestTable");
                    Assert.AreEqual(affectedSelectScalar, 0);
                    Assert.Pass();
                }
                finally
                {
                    connection.ExecuteNonQuery($@"DROP TABLE TestTable");
                }
            }
        }
    }
}