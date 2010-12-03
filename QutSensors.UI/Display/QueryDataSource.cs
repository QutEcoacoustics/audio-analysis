namespace QutSensors.UI.Display
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.UI;

    using QutSensors.Shared;

    public class QueryDataSource : DataSourceControl
    {
        #region Properties
        // With C# 4 with support for covariance this would be IQueryably<object> and remove the need for the reflection based activator
        public IQueryable Query { get; set; }
        #endregion

        protected override DataSourceView GetView(string viewName)
        {
            Validation.Begin()
                        .IsStateNotNull(Query, "Query must be provided")
                        .Check();

            var queryType = Query.GetType().GetGenericArguments()[0];
            var viewType = typeof(QueryDataView<>).MakeGenericType(queryType);
            return (DataSourceView)Activator.CreateInstance(viewType, this, viewName);
        }

        public class QueryDataView<T> : DataSourceView
        {
            private QueryDataSource owner;

            public QueryDataView(QueryDataSource owner, string viewName)
                : base(owner, viewName)
            {
                this.owner = owner;
                Query = (IQueryable<T>)owner.Query;
            }

            #region Properties
            public IQueryable<T> Query { get; set; }
            public override bool CanPage { get { return true; } }
            public override bool CanSort { get { return true; } }
            public override bool CanRetrieveTotalRowCount { get { return true; } }
            #endregion

            protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
            {
                if (arguments.RetrieveTotalRowCount)
                    arguments.TotalRowCount = Query.Count();
                var query = Query;
                if (!string.IsNullOrEmpty(arguments.SortExpression))
                    query = Sort(query, arguments.SortExpression);
                if (arguments.MaximumRows > 0)
                    query = query.Page(arguments.StartRowIndex, arguments.MaximumRows);
                return query;
            }

            IQueryable<T> Sort(IQueryable<T> source, string sortExpression)
            {
                string[] sortParts = sortExpression.Split(' ');
                var param = Expression.Parameter(typeof(T), string.Empty);
                var property = Expression.Property(param, sortParts[0]);

                var funcType = typeof(Func<,>).MakeGenericType(typeof(T), property.Type);
                var method = GetGenericMethod(typeof(Expression), "Lambda", m => !m.GetParameters()[1].ParameterType.IsGenericType).MakeGenericMethod(funcType);
                object sortLambda = method.Invoke(null, new object[] { property, new ParameterExpression[] { param } });

                MethodInfo orderByMethod;
                if (sortParts.Length > 1 && sortParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                    orderByMethod = GetOrderByMethod(sortLambda.GetType(), true);
                else
                    orderByMethod = GetOrderByMethod(sortLambda.GetType(), false);
                orderByMethod = orderByMethod.MakeGenericMethod(typeof(T), property.Type);
                return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { source, sortLambda });
            }

            MethodInfo GetOrderByMethod(Type expressionType, bool descending)
            {
                if (descending)
                    return GetGenericMethod(typeof(Queryable), "OrderByDescending", m => m.GetParameters().Length == 2);
                else
                    return GetGenericMethod(typeof(Queryable), "OrderBy", m => m.GetParameters().Length == 2);
            }

            MethodInfo GetGenericMethod(Type t, string name)
            {
                return GetGenericMethod(t, name, m => true);
            }

            MethodInfo GetGenericMethod(Type t, string name, Predicate<MethodInfo> predicate)
            {
                return t.GetMethods().Where(m => m.Name == name && m.IsGenericMethod && predicate(m)).Single();
            }
        }
    }
}