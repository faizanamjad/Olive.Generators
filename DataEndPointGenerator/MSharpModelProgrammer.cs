﻿using Olive;
using Olive.Entities;
using System;
using System.Linq;
using System.Text;

namespace OliveGenerator
{
    internal class MSharpModelProgrammer
    {
        ReplicatedDataType ReplicatedDataType;

        public MSharpModelProgrammer(ReplicatedDataType replicatedDataType) => ReplicatedDataType = replicatedDataType;

        internal string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("using MSharp;");
            r.AppendLine("namespace PeopleService");
            r.AppendLine("{");
            r.AppendLine();
            r.AppendLine("public class " + ReplicatedDataType.Type.Name + " : EntityType");
            r.AppendLine("{");
            r.AppendLine("public " + ReplicatedDataType.Type.Name + "()");
            r.AppendLine("{");
            r.AppendLine("Schema(\"PeopleService\");");

            foreach (var item in ReplicatedDataType.ReplicatedDataObject.Fields)
            {
                r.AppendLine(AddProperty(item));
            }

            r.AppendLine("}");
            r.AppendLine("}");
            r.AppendLine("}");

            return new CSharpFormatter(r.ToString()).Format();
        }

        string AddProperty(ExportedField item)
        {
            var type = item.Property.PropertyType;
            var name = item.Property.Name;
            if (type.IsArray) type = type.GetElementType();

            bool isNullable;
            if (isNullable = type.IsNullable())
            {
                type = type.GetGenericArguments().Single();
            }

            var method = type.Name;

            //if (item.Property.PropertyType.IsA<IEntity>())
            if (item.IsAssociation)
            {
                method = "Associate" + "<" + type.Name + ">";
                if (type.IsEnum) method = "String";
            }

            switch (method)
            {
                case "Boolean": method = "Bool"; break;
                case "Int32": method = "Int"; break;
                case "Int64": method = "Decimal"; break;
                default: break;
            }

            var result = method + "(\"" + name + "\")";

            if (type.Assembly == Context.AssemblyObject && type.IsArray)
                result += ".MaxCardinality(null)";

            if (!isNullable)
            {
                if (type.IsValueType) result += ".Mandatory()";
            }

            return result + ";";
        }
    }
}