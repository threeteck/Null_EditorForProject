using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorForProject
{
    public class RequiredHtmlValidator : IDataAnnotationHtmlValidator<RequiredAttribute>
    {
        public void ValidateFor(TagBuilder tag, RequiredAttribute required)
        {
            var name = tag.Attributes["name"];
            tag.Attributes.Add("data-val-required",
                required.ErrorMessage ?? $"The field {name} is required.");
        }

        public void ValidateFor(TagBuilder tag, object attribute)
        {
            ValidateFor(tag, (RequiredAttribute) attribute);
        }
    }
}