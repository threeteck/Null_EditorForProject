using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorForProject
{
    public class RangeHtmlValidator : IDataAnnotationHtmlValidator<RangeAttribute>
    {
        public void ValidateFor(TagBuilder tag, RangeAttribute range)
        {
            var name = tag.Attributes["name"]; 
            var min = range.Minimum;
            var max = range.Maximum;
            tag.Attributes.Add("data-val-range",
                range.ErrorMessage ?? $"The field {name} must be between {min} and {max}.");
            tag.Attributes.Add("data-val-range-min", min.ToString());
            tag.Attributes.Add("data-val-range-max", max.ToString());
        }

        public void ValidateFor(TagBuilder tag, object attribute)
        {
            ValidateFor(tag, (RangeAttribute) attribute);
        }
    }
}