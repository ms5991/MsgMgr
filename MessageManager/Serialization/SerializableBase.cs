using MsgMgr.Serialization;
using MsgMgrCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Serialization
{
    /// <summary>
    /// Supported methods of Serialization.  Each enum member should have a corresponding SerializationProvider defined
    /// </summary>
    public enum SerializationType
    {
        XML,
        JSON
    }

    /// <summary>
    /// Derived types are automatically serializable using supported SerializableTypes
    /// </summary>
    public abstract class SerializableBase
    {
        /// <summary>
        /// Mapping from Type to a list of properties of that type that are validly serializable
        /// </summary>
        private static Dictionary<Type, IEnumerable<PropertyInfo>> _propertyCache = new Dictionary<Type, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// Gets the property information for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPropertyInfo(Type type)
        {
            return _propertyCache[type];
        }

        static SerializableBase()
        {
            // find all loaded assemblies
            IEnumerable<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // foreach assembly, find types derived from SerializableBase
            foreach (Assembly assembly in allAssemblies)
            {
                IEnumerable<Type> allSerializableTypes = assembly.GetTypes().Where((Type t) => t.IsSubclassOf(typeof(SerializableBase)));

                // foreach serializable type, find all serializable properties
                foreach (Type type in allSerializableTypes)
                {
                    IEnumerable<PropertyInfo> allProps = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where((PropertyInfo prop) => Attribute.IsDefined(prop, typeof(SerializablePropertyAttribute)));
                    
                    _propertyCache.Add(type, allProps);
                }
            }
        }

        /// <summary>
        /// Deserializes a SerializableBase object from the given byte array .
        /// </summary>
        /// <param name="asciiRep">The ASCII rep.</param>
        /// <param name="serType">Type of serialization to use.</param>
        /// <returns></returns>
        public static SerializableBase Deserialize(byte[] asciiRep, SerializationType serType)
        {
            IDeserializationProvider deserializationProvider = DeserializationProviderBase.GetProvider(serType);

            return deserializationProvider.Deserialize(asciiRep);
        }

        /// <summary>
        /// Gets the string representation of the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="serType">Type of the ser.</param>
        /// <returns></returns>
        public static string GetStringRepresentation(SerializableBase obj, SerializationType serType)
        {
            return obj.Serialize();
        }

        /// <summary>
        /// The serialization provider
        /// </summary>
        private ISerializationProvider _serializationProvider;

        /// <summary>
        /// Serializes this object using the specified type of serialization
        /// </summary>
        /// <param name="serType">Type of the serialization to use.</param>
        /// <returns></returns>
        public byte[] Serialize(SerializationType serType)
        {
            // obtain a SerializationProvider of the given type
            _serializationProvider = SerializationProviderBase.GetProvider(this.GetType().FullName, serType);

            return Encoding.ASCII.GetBytes(Serialize());
        }

        /// <summary>
        /// Private core method to serializes this instance.
        /// </summary>
        /// <returns></returns>
        private string Serialize()
        {
            // start the provider
            _serializationProvider.Begin();

            // serialize each serializable property using reflection
            foreach (PropertyInfo property in _propertyCache[this.GetType()])
            {
                _serializationProvider.AppendObject(property.PropertyType, property.Name, property.GetValue(this));
            }

            // complete serialization
            return _serializationProvider.End();
        }
    }
}
