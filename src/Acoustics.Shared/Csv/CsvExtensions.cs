// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsvExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
        public static MemberMap GetPropertyMap<T>(this ClassMap<T> classMap, Expression<Func<T, object>> propertyExpression)
        {
            var property = propertyExpression.GetProperty();
            return classMap.MemberMaps.Single(cpm => (cpm.Data.Member as PropertyInfo).PropertyInfoMetaDataEquality(property));
        }
    }
}
