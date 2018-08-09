using MsgMgrCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MsgMgr.Serialization
{
    internal class XmlSerializationProvider : SerializationProviderBase
    {
        #region Fields

        /// <summary>
        /// The document that will be constructed
        /// </summary>
        private XDocument document;

        public override SerializationType SerializationType { get { return SerializationType.XML; } }

        #endregion

        #region Constructor

        internal XmlSerializationProvider(string name) : base(name)
        {
        }

        #endregion

        #region Serialization Public

        /// <summary>
        /// Begins this instance by creating a root element with the given name.
        /// </summary>
        public override void Begin()
        {
            document = new XDocument(new XElement(this.Name));
        }

        /// <summary>
        /// Appends an object of some type to the XML document..
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public override void AppendObject(Type objectType, string propertyName, object value)
        {
            if (objectType.IsPrimitive)
            {
                AppendPrimative(propertyName, value);
            }
            else if (objectType == typeof(string))
            {
                AppendString(propertyName, value.ToString());
            }
            else if (objectType == typeof(Guid))
            {
                AppendGuid(propertyName, (Guid)value);
            }
            else if (objectType == typeof(DateTime))
            {
                AppendDateTime(propertyName, (DateTime)value);
            }
            else
            {
                throw new NotImplementedException("Cannot serialize property with name [{0}] and type [{1}]: type not supported".FormatStr(propertyName, objectType.Name));
            }
        }

        public override string End()
        {
            return document.ToString();
        }

        /// <summary>
        /// Appends a primative to the XML document by calling ToString().
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="primative">The primative.</param>
        private void AppendPrimative(string propertyName, object primative)
        {
            AppendXElement(propertyName, primative.ToString(), primative.GetType());
        }

        /// <summary>
        /// Appends a string to the XML document.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="str">The string.</param>
        private void AppendString(string propertyName, string str)
        {
            AppendXElement(propertyName, str, typeof(string));
        }

        /// <summary>
        /// Appends the date time.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="datetime">The datetime.</param>
        private void AppendDateTime(string propertyName, DateTime datetime)
        {
            AppendXElement(propertyName, datetime.ToString("u"), typeof(DateTime));
        }

        /// <summary>
        /// Appends the unique identifier.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="guid">The unique identifier.</param>
        private void AppendGuid(string propertyName, Guid guid)
        {
            AppendXElement(propertyName, guid.ToString(), typeof(Guid));
        }

        #endregion

        #region Serialization Private

        /// <summary>
        /// Appends the x element to the document.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="propType">Type of the property.</param>
        private void AppendXElement(string name, string value, Type propType)
        {
            XElement element = new XElement(name, value);

            XAttribute attribute = new XAttribute("Type", propType.FullName);

            element.Add(attribute);

            document.Root.Add(element);
        }

        #endregion

    }
}
