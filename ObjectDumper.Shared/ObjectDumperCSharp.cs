﻿using System.Collections;
using System.Linq;
using System.Reflection;

namespace System.Diagnostics
{
    /// <summary>
    ///     Source: http://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
    /// </summary>
    public class ObjectDumperCSharp : DumperBase
    {
        public ObjectDumperCSharp(int indentSize = 2) : base(indentSize)
        {
        }

        public static string Dump(object element)
        {
            return Dump(element, 2);
        }

        public static string Dump(object element, int indentSize)
        {
            var instance = new ObjectDumperCSharp(indentSize);
            if (element == null)
            {
                instance.Write("null");
            }
            else
            {
                instance.Write($"var {instance.GetClassName(element).ToLower().Replace("<", "").Replace(">", "")} = ");
                instance.FormatValue(element);
                instance.Write(";");
            }

            return instance.ToString();
        }

        private void CreateObject(object o)
        {
            this.StartLine($"new {this.GetClassName(o)}");
            this.LineBreak();
            this.StartLine("{");
            this.LineBreak();
            this.Level++;

            var properties = o.GetType().GetRuntimeProperties().ToList();
            var last = properties.LastOrDefault();
            foreach (var property in properties)
            {
                var value = property.GetValue(o);
                if (value != null)
                {
                    this.StartLine($"{property.Name} = ");
                    this.FormatValue(value);
                    if (!Equals(property, last))
                    {
                        this.Write(",");
                    }
                    this.LineBreak();
                }
            }
            this.Level--;
            this.StartLine("}");
        }

        /*
        private string DumpElement(object element)
        {
            this.FormatValue(element);
            return "";
            if (element == null || element is ValueType || element is string)
            {
                this.FormatValue(element);
            }
            else
            {
                var objectType = element.GetType();
                if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()))
                {
                    this.Write($"new {objectType.Namespace}.{objectType.Name}()");
                    this.Write("{");
                    this.AddAlreadyTouched(element);
                    this.Level++;
                }

                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (var item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            this.Level++;
                            this.DumpElement(item);
                            this.Level--;
                            this.Write("}");
                        }
                        else
                        {
                            if (!this.AlreadyTouched(item))
                            {
                                this.DumpElement(item);
                            }
                            else
                            {
                                this.Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
                            }
                        }
                    }
                }
                else
                {
                    var publicFields = element.GetType().GetRuntimeFields().Where(f => !f.IsPrivate);
                    foreach (var fieldInfo in publicFields)
                    {
                        object value;
                        try
                        {
                            value = fieldInfo.GetValue(element);
                        }
                        catch (Exception ex)
                        {
                            value = $"{{{ex.Message}}}";
                        }

                        if (fieldInfo.FieldType.GetTypeInfo().IsValueType || fieldInfo.FieldType == typeof(string))
                        {
                            this.Write("{0} = ", fieldInfo.Name);
                            this.FormatValue(value);
                            this.Write(",\n\r");
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).GetTypeInfo()
                                .IsAssignableFrom(fieldInfo.FieldType.GetTypeInfo());
                            this.Write("{0} = {1}", fieldInfo.Name, isEnumerable ? "" : "{ }");

                            var alreadyTouched = !isEnumerable && this.AlreadyTouched(value);
                            this.Level++;
                            if (!alreadyTouched)
                            {
                                this.DumpElement(value);
                            }
                            else
                            {
                                this.Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                            }

                            this.Level--;
                            this.Write("}");
                        }
                    }

                    var publicProperties = element.GetType().GetRuntimeProperties()
                        .Where(p => p.GetMethod != null && p.GetMethod.IsStatic == false);
                    foreach (var propertyInfo in publicProperties)
                    {
                        var type = propertyInfo.PropertyType;
                        object value;
                        try
                        {
                            value = propertyInfo.GetValue(element, null);
                        }
                        catch (Exception ex)
                        {
                            value = $"{{{ex.Message}}}";
                        }

                        if (type.GetTypeInfo().IsValueType || type == typeof(string))
                        {
                            this.Write("{0} = ", propertyInfo.Name);
                            this.FormatValue(value);
                            this.Write(",\n\r");
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
                            this.Write("{0} = {1}", propertyInfo.Name, isEnumerable ? "" : "{ }");

                            var alreadyTouched = !isEnumerable && this.AlreadyTouched(value);
                            this.Level++;
                            if (!alreadyTouched)
                            {
                                this.DumpElement(value);
                            }
                            else
                            {
                                this.Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                            }

                            this.Level--;
                            this.Write("}");
                        }
                    }
                }

                if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()))
                {
                    this.Level--;
                    this.Write("}");
                }
            }

            return this.ToString();
        }
        */

        private void FormatValue(object o)
        {
            if (o is bool)
            {
                this.Write($"{o.ToString().ToLower()}");
                return;
            }

            if (o is string)
            {
                this.Write($"\"{o}\"");
                return;
            }

            if (o is int)
            {
                this.Write($"{o}");
                return;
            }

            if (o is decimal)
            {
                this.Write($"{o}m");
                return;
            }

            if (o is DateTime)
            {
                this.Write($"DateTime.Parse(\"{o}\")");
                return;
            }

            if (o is Enum)
            {
                this.Write($"{o.GetType().FullName}.{o}");
                return;
            }

            if (o is IEnumerable)
            {
                this.Write($"new {this.GetClassName(o)}");
                this.LineBreak();
                this.StartLine("{");
                this.LineBreak();
                this.WriteItems((IEnumerable)o);
                this.StartLine("}");
                return;
            }

            this.CreateObject(o);
        }

        private void WriteItems(IEnumerable items)
        {
            this.Level++;
            var e = items.GetEnumerator();
            if (e.MoveNext())
            {
                this.FormatValue(e.Current);

                while (e.MoveNext())
                {
                    this.Write(",");
                    this.LineBreak();

                    this.FormatValue(e.Current);
                }

                this.LineBreak();
            }

            this.Level--;
        }

        private string GetClassName(object o)
        {
            var type = o.GetType();

            if (type.GetTypeInfo().IsGenericType)
            {
                var arg = type.GetTypeInfo().GenericTypeArguments.First().Name;
                return type.Name.Replace("`1", $"<{arg}>");
            }

            return type.Name;
        }
    }
}