using MsgMgrCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Serialization
{
    /// <summary>
    /// Abstract base class for serialization
    /// </summary>
    /// <seealso cref="MsgMgr.Serialization.ISerializationProvider" />
    internal abstract class SerializationProviderBase : ISerializationProvider
    {
        /// <summary>
        /// Gets a provider corresponding to the serialization type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unknown serialization type: [{0}]".FormatStr(type)</exception>
        public static ISerializationProvider GetProvider(string name, SerializationType type)
        {
            SerializationProviderBase provider;

            switch (type)
            {
                case SerializationType.XML:
                    provider = new XmlSerializationProvider(name);
                    break;
                case SerializationType.JSON:
                    provider = new JsonSerializationProvider(name);
                    break;
                default:
                    throw new InvalidOperationException("Unknown serialization type: [{0}]".FormatStr(type));
            }

            return provider;
        }

        /// <summary>
        /// Represents the type name of the object being serialized.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        public abstract SerializationType SerializationType { get; }

        protected SerializationProviderBase(string name)
        {
            Name = name;
        }

        public abstract void Begin();
        public abstract void AppendObject(Type objectType, string propertyName, object value);
        public abstract string End();
    }
}
