using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acoustics.Shared.Extensions
{
    using Microsoft.FSharp.Core;

    public static class FSharpExtensions
    {
        public static FSharpOption<T> ToOption<T>(this T obj)
        {
            if (obj == null)
            {
                return FSharpOption<T>.None;
            }

            return FSharpOption<T>.Some(obj);
        }
    }
}
