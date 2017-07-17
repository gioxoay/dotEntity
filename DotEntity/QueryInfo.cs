﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (QueryInfo.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
namespace DotEntity
{
    public class QueryInfo
    {
        public string LinkingOperator { get; set; }
        public string PropertyName { get; set; }
        public string ParameterName { get; set; }
        public object PropertyValue { get; set; }
        public string QueryOperator { get; set; }
        public bool IsPropertyValueAlsoProperty { get; set; }
        public bool SupportOperator { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryInfo" /> class.
        /// </summary>
        /// <param name="linkingOperator">The linking operator.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="queryOperator">The query operator.</param>
        internal QueryInfo(string linkingOperator, string propertyName, object propertyValue, string queryOperator, string parameterName, bool isPropertyValueAlsoAProperty = false)
        {
            LinkingOperator = linkingOperator;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            QueryOperator = queryOperator;
            ParameterName = parameterName;
            IsPropertyValueAlsoProperty = isPropertyValueAlsoAProperty;
        }

        internal QueryInfo(bool supportOperator, string linkingOperator)
        {
            SupportOperator = supportOperator;
            LinkingOperator = linkingOperator;
        }

        public override string ToString()
        {
            return $@"Query Info:
                        Linking Operator: {LinkingOperator}
                        Query Operator: {QueryOperator}
                        Property Name: {PropertyName}
                        Property Value: {PropertyValue}
                        Parameter Name: {ParameterName}
                        Is Property: {IsPropertyValueAlsoProperty}
                        Support: {SupportOperator}";
        }
    }
}