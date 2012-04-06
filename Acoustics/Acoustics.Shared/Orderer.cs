namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Sort direction.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ascending sort order.
        /// </summary>
        [Description("ASC")]
        Ascending,

        /// <summary>
        /// Descending sort order.
        /// </summary>
        [Description("DESC")]
        Descending
    }

    /// <summary>
    /// Orderer interface.
    /// </summary>
    /// <typeparam name="TOrder">Type to order.</typeparam>
    /// <remarks>
    /// see: http://stackoverflow.com/questions/7737355/store-multi-type-orderby-expression-as-a-property/7742446#7742446 for more info.
    /// </remarks>
    public interface IOrderer<TOrder>
    {
        /// <summary>
        /// Apply order by to IQueryable.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// Ordered Queryable.
        /// </returns>
        IOrderedQueryable<TOrder> ApplyOrderBy(IQueryable<TOrder> source);

        /// <summary>
        /// Apply order then by to IQueryable.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// Ordered Queryable.
        /// </returns>
        IOrderedQueryable<TOrder> ApplyThenBy(IOrderedQueryable<TOrder> source);
    }

    /// <summary>
    /// Implementation of IOrderer.
    /// </summary>
    /// <typeparam name="TOrder">
    /// Type to order.
    /// </typeparam>
    /// <typeparam name="TOrderProperty">
    /// Type from property to order by.
    /// </typeparam>
    public class Orderer<TOrder, TOrderProperty> : IOrderer<TOrder>
    {
        private readonly Expression<Func<TOrder, TOrderProperty>> orderExpr;

        private readonly SortDirection sortDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Orderer{TOrder,TOrderProperty}"/> class. Orders by ascending.
        /// </summary>
        /// <param name="orderExpr">
        /// The order expr.
        /// </param>
        public Orderer(Expression<Func<TOrder, TOrderProperty>> orderExpr)
            : this(orderExpr, SortDirection.Ascending)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Orderer{TOrder,TOrderProperty}"/> class. 
        /// </summary>
        /// <param name="orderExpr">
        /// The order expr.
        /// </param>
        /// <param name="sortDirection">
        /// The sort Direction.
        /// </param>
        public Orderer(Expression<Func<TOrder, TOrderProperty>> orderExpr, SortDirection sortDirection)
        {
            this.orderExpr = orderExpr;
            this.sortDirection = sortDirection;
        }

        /// <summary>
        /// Apply order by to IQueryable.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// Ordered Queryable.
        /// </returns>
        public IOrderedQueryable<TOrder> ApplyOrderBy(IQueryable<TOrder> source)
        {
            return this.sortDirection == SortDirection.Ascending
                       ? source.OrderBy(this.orderExpr)
                       : source.OrderByDescending(this.orderExpr);
        }

        /// <summary>
        /// Apply order then by to IQueryable.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// Ordered Queryable.
        /// </returns>
        public IOrderedQueryable<TOrder> ApplyThenBy(IOrderedQueryable<TOrder> source)
        {
            return this.sortDirection == SortDirection.Ascending
                       ? source.ThenBy(this.orderExpr)
                       : source.ThenByDescending(this.orderExpr);
        }
    }

    /// <summary>
    /// Order coordinator.
    /// </summary>
    /// <typeparam name="T">
    /// Type to order.
    /// </typeparam>
    public class OrderCoordinator<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCoordinator{T}"/> class.
        /// </summary>
        public OrderCoordinator()
        {
            this.Orders = new List<IOrderer<T>>();
        }

        /// <summary>
        /// Gets or sets Orders.
        /// </summary>
        public List<IOrderer<T>> Orders { get; set; }

        /// <summary>
        /// Apply orders.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// Queryable ordered by Orders.
        /// </returns>
        /// <remarks>
        /// note, did not return IOrderedQueryable to support ability to return with empty Orders.
        /// </remarks>
        public IQueryable<T> ApplyOrders(IQueryable<T> source)
        {
            if (this.Orders == null || this.Orders.Count < 1)
            {
                return source;
            }

            // order by the first orderer
            var orderedSource = this.Orders.First().ApplyOrderBy(source);

            // then by the rest of the orderers
            foreach (IOrderer<T> orderer in this.Orders.Skip(1))
            {
                orderedSource = orderer.ApplyThenBy(orderedSource);
            }

            return orderedSource;
        }

        /// <summary>
        /// Apply orders.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// Queryable ordered by Orders.
        /// </returns>
        /// <remarks>
        /// note, did not return IOrderedQueryable to support ability to return with empty Orders.
        /// </remarks>
        public IOrderedQueryable<T> ApplyOrders(IOrderedQueryable<T> source)
        {
            if (this.Orders == null || this.Orders.Count < 1)
            {
                return source;
            }

            foreach (IOrderer<T> orderer in this.Orders.Skip(1))
            {
                source = orderer.ApplyThenBy(source);
            }

            return source;
        }
    }

    internal class ExampleOrderCoordinator
    {
        public void Test()
        {
            OrderCoordinator<FileInfo> coord = new OrderCoordinator<FileInfo>();
            coord.Orders.Add(new Orderer<FileInfo, string>(c => c.Name, SortDirection.Ascending));
            coord.Orders.Add(new Orderer<FileInfo, long>(c => c.Length, SortDirection.Ascending));

            IQueryable<FileInfo> query = Enumerable.Empty<FileInfo>().AsQueryable();

            query = coord.ApplyOrders(query);

            string result = query.Expression.ToString();
        }
    }

}