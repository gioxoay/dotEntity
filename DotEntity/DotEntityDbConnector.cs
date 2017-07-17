﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DotEntityDbConnector.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Data;
using DotEntity.Enumerations;

namespace DotEntity
{
    internal static class DotEntityDbConnector
    {
        public static void ExecuteCommand(DotEntityDbCommand command, bool useTransaction = false, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            ExecuteCommands(new[] {command}, useTransaction, isolationLevel);
        }

        public static void ExecuteCommands(DotEntityDbCommand[] commands, bool useTransaction = false, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            var queryProcessor = QueryProcessor.Instance;
            using (var con = DotEntityDb.Provider.Connection)
            {
                con.Open();
                using (var trans = useTransaction ? con.BeginTransaction(isolationLevel) : null)
                {
                    foreach (var DotEntityDbCommand in commands)
                    {
                        switch (DotEntityDbCommand.OperationType)
                        {
                            case DbOperationType.Insert:
                            case DbOperationType.SelectScaler:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos, true, DotEntityDbCommand.KeyColumn))
                                {
                                    cmd.Transaction = trans;
                                    var value = cmd.ExecuteScalar();

                                    DotEntityDbCommand.SetRawResult(value);
                                }
                                break;
                            case DbOperationType.Update:
                            case DbOperationType.Delete:
                            case DbOperationType.Other:    
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos))
                                {
                                    cmd.Transaction = trans;
                                    DotEntityDbCommand.SetRawResult(cmd.ExecuteNonQuery());
                                }
                                break;
                            case DbOperationType.Select:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos))
                                {
                                    cmd.Transaction = trans;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        DotEntityDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            case DbOperationType.MultiQuery:
                                //the difference between a multiquery and select is that in multiquery,
                                //we don't dispose the reader immediately. It's the responsibility of the
                                //reader processor to dispose it manually
                                //todo: Can we have a better solution than this?
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos))
                                {
                                    cmd.Transaction = trans;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        DotEntityDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            case DbOperationType.Procedure:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos, commandType: CommandType.StoredProcedure))
                                {
                                    cmd.Transaction = trans;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        DotEntityDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if(!DotEntityDbCommand.ContinueNextCommand)
                            trans?.Rollback();
                        
                    }
                    if (trans?.Connection != null)
                        trans.Commit();
                }

            }
        }
    }
}