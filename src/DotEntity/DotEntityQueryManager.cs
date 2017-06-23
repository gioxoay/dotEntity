﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DotEntityQueryManager.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity or dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotEntity.Enumerations;
using DotEntity.Extensions;

namespace DotEntity
{
    internal class DotEntityQueryManager : IDotEntityQueryManager
    {
        private readonly IQueryGenerator _queryGenerator;
        internal DotEntityQueryManager()
        {
            _queryGenerator = DotEntityDb.Provider.QueryGenerator;
        }

        public virtual TType DoScaler<TType>(string query, dynamic parameters, Func<TType, bool> resultAction = null)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.SelectScaler, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<TType>();
        }

        public virtual IEnumerable<T> Do<T>(string query, dynamic parameters, Func<IEnumerable<T>, bool> resultAction = null) where T : class
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader => DataDeserializer<T>.Instance.DeserializeMany(reader));
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual void Do(string query, dynamic parameters)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
        }

        public virtual int DoInsert<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var query = _queryGenerator.GenerateInsert(entity, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Insert, query, queryParameters, keyColumn);
            cmd.ProcessResult(result =>
            {
                var id = result as int? ?? Convert.ToInt32(result);
                DataDeserializer<T>.Instance.SetPropertyAs<int>(entity, keyColumn, id);
            });
            DotEntityDbConnector.ExecuteCommand(cmd);
            return 1;
        }

        public virtual int DoUpdate<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateUpdate(entity, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Update, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoUpdate<T>(dynamic entity, Expression<Func<T, bool>> where, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateUpdate(entity, where, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Update, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoDelete<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateDelete(entity, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Delete, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoDelete<T>(Expression<Func<T, bool>> where, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateDelete<T>(where, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Delete, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual IEnumerable<T> DoSelect<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy, page, count);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                return DataDeserializer<T>.Instance.DeserializeMany(reader);
            });
            DotEntityDbConnector.ExecuteCommand(cmd);

            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual Tuple<int, IEnumerable<T>> DoSelectWithTotalMatches<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateSelectWithTotalMatchingCount(out IList<QueryInfo> queryParameters, where, orderBy, page, count);

            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                var ts = DataDeserializer<T>.Instance.DeserializeMany(reader);
                reader.NextResult();
                reader.Read();
                var fv = reader[0];
                var total = fv as int? ?? Convert.ToInt32(fv);
                return Tuple.Create(total, ts);
            });
            DotEntityDbConnector.ExecuteCommand(cmd);

            return cmd.GetResultAs<Tuple<int, IEnumerable<T>>>();
        }

        public virtual T DoSelectSingle<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null) where T : class
        {
            var query = _queryGenerator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader => DataDeserializer<T>.Instance.DeserializeSingle(reader));
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<T>();
        }

        public virtual int DoCount<T>(List<Expression<Func<T, bool>>> where, Func<int, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateCount(where, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.SelectScaler, query, queryParameters);
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        private static void ThrowIfInvalidPage(ICollection orderBy, int page, int count)
        {
            Throw.IfInvalidPagination(orderBy, page, count);
        }

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
        }

        public bool IsDisposed()
        {
            return _disposed;
        }
    }
}