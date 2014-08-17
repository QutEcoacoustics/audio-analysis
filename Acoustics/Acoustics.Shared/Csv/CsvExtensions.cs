// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisProgramsClassMapper.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the AnalysisProgramsClassMapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    using CsvHelper.Configuration;

    public static class CsvExtensions
    {
        public static CsvPropertyMap GetPropertyMap<T>(this CsvClassMap<T> classMap, Expression<Func<T, object>> propertyExpression)
        {
            var property = propertyExpression.GetProperty();
            return classMap.PropertyMaps.Single(cpm => cpm.Data.Property.PropertyInfoMetaDataEquality(property));
        }
    }
}
