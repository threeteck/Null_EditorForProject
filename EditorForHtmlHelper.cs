using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorForProject
{
    public static class EditorForHtmlHelper
    {
        public static IHtmlContent SimpleEditorFor<TSource, TResult>(
            this IHtmlHelper<TSource> helper
            , Expression<Func<TSource, TResult>> selectorExpression, bool withLabel = false, string name = null, params string[] classes)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException(nameof(selectorExpression));
            if (selectorExpression.Body is ConstantExpression)
                throw new ArgumentException("You cannot generate an editor for a constant value");

            return new EditorForHtmlGenerator<TSource>(helper).VisitExpression(selectorExpression.Body, withLabel, name, classes);
        }
    }
}