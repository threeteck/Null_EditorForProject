using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorForProject
{
    public interface IDataAnnotationHtmlValidator<TAttribute> : IDataAnnotationHtmlValidator
        where TAttribute : ValidationAttribute
    {
        void ValidateFor(TagBuilder tag, TAttribute attribute);
    }

    public interface IDataAnnotationHtmlValidator
    {
        void ValidateFor(TagBuilder tag, object attribute);
    }
}