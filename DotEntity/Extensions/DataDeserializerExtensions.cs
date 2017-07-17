﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DataDeserializerExtensions.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

namespace DotEntity.Extensions
{
    internal static class DataDeserializerExtensions
    {
        internal static string[] GetTypedColumnNames(this IDataDeserializer deserializer, string[] columns, Type type)
        {
            var typedColumns = new string[columns.Length];
            var typeName = type.Name;
            for (var i = 0; i < columns.Length; i++)
                typedColumns[i] = typeName + "." + columns[i];

            return typedColumns;
        }
    }
}