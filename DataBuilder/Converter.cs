using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using RDapter.DataBuilder.Helper;
using RDapter.DataBuilder.Interface;

namespace RDapter.DataBuilder
{
    /// <summary>
    /// alternative to reflection builder with MUCH better on performance, implementation taken from https://stackoverflow.com/questions/19841120/generic-dbdatareader-to-listt-mapping/19845980#19845980
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Converter<T> : IDataMapper<T> where T : new()
    {
        // ReSharper disable once InconsistentNaming
        private static Func<IDataReader, T> _converter;
        private readonly IDataReader _dataReader;

        private Func<IDataReader, T> CompileMapperFunction()
        {
            var expressions = new List<Expression>();

            var dataReaderParameter = Expression.Parameter(typeof(IDataRecord), "datareader");

            var objectInstance = Expression.Variable(typeof(T));
            //var target = new T();
            expressions.Add(Expression.Assign(objectInstance, Expression.New(objectInstance.Type)));

            //does int based lookup
            var indexerInfo = typeof(IDataRecord).GetProperty("Item", new[] { typeof(int) });

            var columns = Enumerable.Range(0, _dataReader.FieldCount).Select(i => new { index = i, name = _dataReader.GetName(i), type = _dataReader.GetFieldType(i) });
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var column in columns)
            {
                //var property = properties.FirstOrDefault(x => x.Name == column.name);
                var property = properties.SingleOrDefault(x => column.name == Global.GetDefaultTypeMap<T>(x.Name) && column.type == x.PropertyType);
                if (property == null)
                    continue;

                var index = Expression.Constant(column.index);
                // equivalent to datareader[(int)readerIndex] where datareader is incoming parameter.
                var value = Expression.MakeIndex(dataReaderParameter, indexerInfo, new[] { index });

                var actualType = _dataReader.GetFieldType(column.index);
                Expression safeCastExpression;
                //if property type in model doesn't match the underlying type in SQL, we first convert into actual SQL type.
                if (actualType != property.PropertyType)
                {
                    safeCastExpression = Expression.Convert(value, actualType);
                }
                //otherwise we do nothing.
                else
                {
                    safeCastExpression = value;
                }
                var isReaderDbNull = Expression.Call(dataReaderParameter, "IsDBNull", null, index);
                var propertyExpression = Expression.Property(objectInstance, property);
                /*
                 if(datareader.IsDBNull((int)readerIndex){
                    objectInstance.property = default;
                 }else{
                    objectInstance.property = (castType)value;
                 }
                 */
                var assignmentBlock = Expression.Condition(
                                            Expression.IsTrue(isReaderDbNull),
                                                Expression.Assign(propertyExpression, Expression.Default(property.PropertyType)),
                                                Expression.Assign(propertyExpression, Expression.Convert(safeCastExpression, property.PropertyType)
                                             )
                                     );
                expressions.Add(assignmentBlock);
            }
            //return objectInstance;
            expressions.Add(objectInstance);
            var func = Expression.Lambda<Func<IDataReader, T>>(Expression.Block(new[] { objectInstance }, expressions), dataReaderParameter);
            return func.Compile();
        }

        public Converter(IDataReader dataReader)
        {
            this._dataReader = dataReader;
            if (_converter != null) return;
            _converter = CompileMapperFunction();
        }
        public T GenerateObject()
        {
            return _converter(_dataReader);
        }
        public IEnumerable<T> GenerateObjects()
        {
            while (_dataReader.Read())
            {
                yield return _converter(_dataReader);
            }
        }
    }
}
