using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataGate.Utils;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorForProject
{
    public class EditorForHtmlGenerator<TModel>
    {
        private IHtmlHelper<TModel> _helper;
        private HashSet<Type> _visited;
        private Dictionary<Type, IDataAnnotationHtmlValidator> _validators;

        public EditorForHtmlGenerator(IHtmlHelper<TModel> helper)
        {
            _helper = helper;
            _visited = new HashSet<Type>();

            _validators = new Dictionary<Type, IDataAnnotationHtmlValidator>();
            
            foreach(var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IDataAnnotationHtmlValidator).IsAssignableFrom(t) 
                            && !t.IsInterface))
            {
                var obj = Activator.CreateInstance(type);
                var attributeType = type.GetInterfaces()
                    .Single(i => i.IsConstructedGenericType 
                                 && i.GetGenericTypeDefinition() == typeof(IDataAnnotationHtmlValidator<>))
                    .GetGenericArguments().Single();

                _validators.Add(attributeType, (IDataAnnotationHtmlValidator)obj);
            }
        }

        public IHtmlContent VisitExpression(Expression expression
            , bool withLabel = false, string name = null, params string[] classes)
        {
            if (expression.Type == typeof(string))
                return SimpleEditorForString(expression, name, classes, withLabel);
            if (expression.Type == typeof(int) || expression.Type == typeof(long))
                return SimpleEditorForNumber(expression, name, classes, withLabel);
            if (expression.Type == typeof(bool))
                return SimpleEditorForBoolean(expression, name, classes, withLabel);
            if (expression.Type.IsEnum)
                return SimpleEditorForEnum(expression, name, classes, withLabel);

            if(_visited.Contains(expression.Type))
                return new HtmlString("");
            _visited.Add(expression.Type);
            var result = SimpleEditorForObject(expression, name, classes, withLabel);
            _visited.Remove(expression.Type);
            return result;
        }

        private object GetValue(Expression expression)
        {
            if (expression is ParameterExpression)
                return _helper.ViewData.Model;
            
            if (expression is MemberExpression memberExpression)
            {

                if (memberExpression.NodeType != ExpressionType.MemberAccess)
                    throw new Exception();

                object obj = null;

                if (memberExpression.Expression is ConstantExpression c)
                    obj = c.Value;

                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                    obj = _helper.ViewData.Model;

                if (memberExpression.Expression is MemberExpression m)
                    obj = GetValue(m);

                var member = memberExpression.Member;
                
                if (obj == null && member.DeclaringType.GetConstructor(Type.EmptyTypes) != null)
                    obj = Activator.CreateInstance(member.DeclaringType);

                if (obj != null)
                    return member.ToVariableInfo().Get(obj);
            }

            return null;
        }

        private string ResolveTagBuilder(TagBuilder tag
            , Expression expression, string name, string[] classes)
        {
            var memberName = name;
            if(memberName == null)
                if(expression is MemberExpression memberExpression)
                    memberName = memberExpression.Member.Name;
                else if (expression is ParameterExpression parameterExpression)
                    memberName = parameterExpression.Name;
            foreach (var @class in classes)
                tag.AddCssClass(@class);
            tag.Attributes.Add("id", memberName);
            return memberName;
        }

        private TagBuilder GetInputTagBuilder(
            Expression expression, string name, string[] classes, object value)
        {
            var inputTagBuilder = new TagBuilder("input");
            var memberName = ResolveTagBuilder(inputTagBuilder, expression, name, classes);
            inputTagBuilder.Attributes.Add("name", memberName);
            inputTagBuilder.Attributes.Add("value", value?.ToString() ?? "");
            return inputTagBuilder;
        }

        private IHtmlContent SimpleEditorForString(Expression expression
            , string name, string[] classes, bool withLabel)
        {
            var value = GetValue(expression);
            var inputTagBuilder = GetInputTagBuilder(expression, name, classes, value);
            inputTagBuilder.Attributes.Add("type", "text");
            
            var result = AssignValidation(inputTagBuilder, expression);
            if (withLabel) 
                result = AssignLabel(result, inputTagBuilder.Attributes["id"], GetDisplayName(expression));
            return result;
        }
        
        private IHtmlContent SimpleEditorForNumber(Expression expression
            , string name, string[] classes, bool withLabel)
        {
            var value = GetValue(expression);
            var inputTagBuilder = GetInputTagBuilder(expression, name, classes, value);
            inputTagBuilder.Attributes.Add("type", "number");
            
            var result = AssignValidation(inputTagBuilder, expression);
            if (withLabel) 
                result = AssignLabel(result, inputTagBuilder.Attributes["id"], GetDisplayName(expression));
            return result;
        }
        
        private IHtmlContent SimpleEditorForBoolean(Expression expression
            , string name, string[] classes, bool withLabel)
        {
            var value = (bool)(GetValue(expression) ?? false);
            var inputTagBuilder = GetInputTagBuilder(expression, name, classes, value);
            inputTagBuilder.Attributes.Add("type", "checkbox");
            if(value)
                inputTagBuilder.Attributes.Add("checked", "checked");
            
            var result = AssignValidation(inputTagBuilder, expression);
            if (withLabel) 
                result = AssignLabel(result, inputTagBuilder.Attributes["id"], GetDisplayName(expression));
            return result;
        }

        private IHtmlContent SimpleEditorForEnum(Expression expression
            , string name, string[] classes, bool withLabel)
        {
            var selectTagBuilder = new TagBuilder("select");
            var value = GetValue(expression);
            var memberName = ResolveTagBuilder(selectTagBuilder, expression, name, classes);
            selectTagBuilder.Attributes.Add("name", memberName);

            foreach (int val in Enum.GetValues(expression.Type))
            {
                var optionTagBuilder = new TagBuilder("option");
                optionTagBuilder.Attributes.Add("value", val.ToString());
                optionTagBuilder.InnerHtml.Append(Enum.GetName(expression.Type, val));
                if (value != null && (int)value == val)
                    optionTagBuilder.Attributes.Add("selected", "");
                selectTagBuilder.InnerHtml.AppendHtml(optionTagBuilder);
            }
            var result = selectTagBuilder;
            if (withLabel) 
                result = AssignLabel(result, memberName, GetDisplayName(expression));
            return result;
        }
        
        private IHtmlContent SimpleEditorForObject(Expression expression
            , string name, string[] classes, bool withLabel)
        {
            var divTagBuilder = new TagBuilder("div");
            var memberName = ResolveTagBuilder(divTagBuilder, expression, name, classes);
            divTagBuilder.AddCssClass("px-5");

            foreach (var variable in expression.Type.GetVariables())
            {
                var htmlContent = VisitExpression(
                    Expression.MakeMemberAccess(expression, variable.MemberInfo)
                    , true);
                
                divTagBuilder.InnerHtml.AppendHtml(htmlContent);
            }

            var result = divTagBuilder;
            if (withLabel) 
                result = AssignLabel(result, memberName, GetDisplayName(expression));
            return result;
        }

        private TagBuilder AssignLabel(IHtmlContent content,string id, string name = null)
        {
            if (name == null)
                name = id;
            var containerTagBuilder = new TagBuilder("div");

            var divTabBuilder = new TagBuilder("div");
            divTabBuilder.AddCssClass("editor-label");
            
            var labelTagBuilder = new TagBuilder("label");
            labelTagBuilder.Attributes.Add("for", id);
            labelTagBuilder.InnerHtml.Append(name);
            
            divTabBuilder.InnerHtml.AppendHtml(labelTagBuilder);

            containerTagBuilder.InnerHtml.AppendHtml(divTabBuilder);
            containerTagBuilder.InnerHtml.AppendHtml(content);

            return containerTagBuilder;
        }

        private string GetDisplayName(Expression expression)
        {
            if (expression is MemberExpression memberExpression
                && memberExpression.Member.GetCustomAttribute<DisplayNameAttribute>() != null)
                return memberExpression.Member.GetCustomAttribute<DisplayNameAttribute>().DisplayName;
            return null;
        }

        private IHtmlContent AssignValidation(TagBuilder inputTag, Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var divTagBuilder = new TagBuilder("div");
                
                inputTag.Attributes.Add("data-val", "true");

                foreach (var attr in member.GetCustomAttributes())
                    GetValidator(attr.GetType())?.ValidateFor(inputTag, attr);

                var spanTagBuilder = new TagBuilder("span");
                spanTagBuilder.AddCssClass("field-validation-error");
                spanTagBuilder.Attributes.Add("data-valmsg-for", inputTag.Attributes["name"]);
                spanTagBuilder.Attributes.Add("data-valmsg-replace", "true");

                divTagBuilder.InnerHtml.AppendHtml(inputTag);
                divTagBuilder.InnerHtml.AppendHtml(spanTagBuilder);

                return divTagBuilder;
            }

            return inputTag;
        }

        private IDataAnnotationHtmlValidator GetValidator(Type attributeType)
        {
            if (_validators.ContainsKey(attributeType))
                return _validators[attributeType];
            return null;
        }
    }
}