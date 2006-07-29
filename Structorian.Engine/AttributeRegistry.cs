using System;
using System.Collections.Generic;
using Structorian.Engine.Fields;

namespace Structorian.Engine
{
    enum AttributeType { Expression };

    class AttributeRegistry
    {
        private Dictionary<Type, Dictionary<string, AttributeType>> myRegistry = new Dictionary<Type, Dictionary<string, AttributeType>>();

        internal AttributeRegistry()
        {
            RegisterAttribute(typeof(AssertField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(BitfieldField), "size", AttributeType.Expression);
            RegisterAttribute(typeof(CalcField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(CaseField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(ChildField), "offset", AttributeType.Expression);
            RegisterAttribute(typeof(ChildField), "count", AttributeType.Expression);
            RegisterAttribute(typeof(GlobalField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(IfField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(NodenameField), "name", AttributeType.Expression);
            RegisterAttribute(typeof(RepeatField), "count", AttributeType.Expression);
            RegisterAttribute(typeof(SeekField), "offset", AttributeType.Expression);
            RegisterAttribute(typeof(StrField), "len", AttributeType.Expression);
            RegisterAttribute(typeof(SwitchField), "expr", AttributeType.Expression);
        }

        internal void RegisterAttribute(Type fieldType, string attrName, AttributeType type)
        {
            Dictionary<string, AttributeType> fieldsForType;
            if (!myRegistry.TryGetValue(fieldType, out fieldsForType))
            {
                fieldsForType = new Dictionary<string, AttributeType>();
                myRegistry.Add(fieldType, fieldsForType);
            }
            fieldsForType.Add(attrName, type);
        }

        internal void SetFieldAttribute(StructField field, string key, string value)
        {
            Type fieldType = field.GetType();
            
            while(true)
            {
                Dictionary<string, AttributeType> fieldsForType;
                if (myRegistry.TryGetValue(fieldType, out fieldsForType))
                {
                    AttributeType attrType;
                    if (fieldsForType.TryGetValue(key, out attrType))
                    {
                        SetFieldAttributeValue(field, key, attrType, value);
                        return;
                    }
                }
                if (fieldType.Equals(typeof(StructField))) break;
                fieldType = fieldType.BaseType;
            }
            
            field.SetAttribute(key, value);
        }

        private void SetFieldAttributeValue(StructField field, string key, AttributeType type, string value)
        {
            switch(type)
            {
                case AttributeType.Expression:
                    field.SetAttributeValue(key, ExpressionParser.Parse(value));
                    break;
            }
        }
    }
}
