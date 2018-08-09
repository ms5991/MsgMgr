using MsgMgrCommon.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MsgMgr.Serialization
{
    internal class XmlDeserializationProvider : DeserializationProviderBase
    {
        private XDocument document;

        /// <summary>
        /// Deserializes a SerializableObject from the provided byte array containing an XDocument.
        /// </summary>
        /// <param name="asciiRep">The ASCII rep.</param>
        /// <returns></returns>
        public override SerializableBase Deserialize(byte[] asciiRep)
        {
            // load the document
            using (MemoryStream ms = new MemoryStream(asciiRep))
            {
                document = XDocument.Load(ms);
            }

            // the type name is the name of the root element
            Type deserializedType = Type.GetType(document.Root.Name.LocalName);

            // create an empty result object of the correct type
            SerializableBase result = (SerializableBase)Activator.CreateInstance(deserializedType);
            
            // deserialize each property
            foreach (XElement element in document.Root.Elements())
            {
                Type elementType = Type.GetType(element.Attribute("Type").Value);

                // use reflection to set the value of this property on the created object
                deserializedType.GetProperty(element.Name.LocalName).DeclaringType.GetProperty(element.Name.LocalName).SetValue(result, ParseObject(elementType, element.Value));
            }

            return result;
        }

        /// <summary>
        /// Does the reverse of AppendObject from SerializationProvider.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stringRep">The string rep.</param>
        /// <returns></returns>
        private object ParseObject(Type type, string stringRep)
        {
            if (type.IsPrimitive)
            {
                return Convert.ChangeType(stringRep, type);
            }
            else if (type == typeof(string))
            {
                return stringRep;
            }
            else if (type == typeof(Guid))
            {
                return new Guid(stringRep);
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.Parse(stringRep);
            }

            throw new NotImplementedException("Cannot deserialize property of type [{0}]: type not supported".FormatStr(type.Name));
        }
    }
}
