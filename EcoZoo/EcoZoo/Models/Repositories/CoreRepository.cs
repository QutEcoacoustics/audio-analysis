using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcoZoo.Models.Repositories
{
    using System.Diagnostics.Contracts;

    using EcoZoo.Models.Util;

    public class CoreRepository<T> where T : class 
    {

        private readonly QutSensorsDb _context;

        public CoreRepository(QutSensorsDb context)
        {
            Contract.Requires(context != null);

            this._context = context;
        }

        public QutSensorsDb Context
        {
            get
            {
                return this._context;
            }
        }
    }
}