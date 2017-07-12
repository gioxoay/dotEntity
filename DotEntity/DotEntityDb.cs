﻿// #region Author Information
// // DotEntity.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using DotEntity.Providers;
using DotEntity.Versioning;

[assembly: InternalsVisibleTo("DotEntity.Tests")]
namespace DotEntity
{
    public partial class DotEntityDb
    {
        public static string ConnectionString { get; set; }

        private static ConcurrentDictionary<Type, string> EntityTableNames { get; set; }

        static DotEntityDb()
        {
            EntityTableNames = new ConcurrentDictionary<Type, string>();
            QueryProcessor = new QueryProcessor();
        }

        public static void Initialize(string connectionString, IDatabaseProvider provider)
        {
            ConnectionString = connectionString;
            Provider = provider;
            Provider.QueryGenerator = Provider.QueryGenerator ?? new DefaultQueryGenerator();
            Provider.TypeMapProvider = Provider.TypeMapProvider ?? new DefaultTypeMapProvider();
            Provider.DatabaseTableGenerator = Provider.DatabaseTableGenerator ?? new DefaultDatabaseTableGenerator();
        }

        public static IDatabaseProvider Provider { get; internal set; }

        internal static QueryProcessor QueryProcessor { get; set; }

        public static void MapTableNameForType<T>(string tableName)
        {
            EntityTableNames.AddOrUpdate(typeof(T), tableName, (type, s) => tableName);
        }

        public static void Relate<TSource, TTarget>(string sourceColumnName, string destinationColumnName)
        {
            RelationMapper.Relate<TSource, TTarget>(sourceColumnName, destinationColumnName);
        }

        public static string GetTableNameForType<T>()
        {
            var tType = typeof(T);
            return GetTableNameForType(tType);
        }

        public static string GetTableNameForType(Type type)
        {
            return EntityTableNames.ContainsKey(type) ? EntityTableNames[type] : type.Name;
        }

        public static void UpdateDatabaseToLatestVersion()
        {
            var versionRunner = new VersionUpdater();
            versionRunner.RunUpgrade();
        }

        public static void UpdateDatabaseToVersion(string versionKey)
        {
            var versionRunner = new VersionUpdater();
            versionRunner.RunDowngrade(versionKey);
        }
    }
}