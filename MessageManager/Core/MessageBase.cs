using Newtonsoft.Json;
using MsgMgr.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Threading;

namespace MsgMgr.Core
{
    [Serializable]
    public abstract class MessageBase
    {
        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        [JsonProperty]
        public Guid Identity { get; private set; }

        /// <summary>
        /// Gets or sets the time sent.
        /// </summary>
        /// <value>
        /// The time sent.
        /// </value>
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Gets or sets the time received.
        /// </summary>
        /// <value>
        /// The time received.
        /// </value>
        public DateTime TimeReceived { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBase"/> class.
        /// </summary>
        protected MessageBase()
        {
            Identity = Guid.NewGuid();
        }

        /// <summary>
        /// Method to be called when the message is received.
        /// </summary>
        public virtual void InvokeOnReceieve()
        {

        }

        #region Serialization

        /// <summary>
        /// Serializes the specified Message.
        /// </summary>
        /// <param name="toSerialize">To serialize.</param>
        /// <returns></returns>
        internal static byte[] Serialize(MessageBase toSerialize)
        {
            // encode to json first so the networking can be cross platform
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                        
            string jsonStr = JsonConvert.SerializeObject(toSerialize, settings);


            return Encoding.ASCII.GetBytes(jsonStr);
        }


        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        internal static MessageBase Deserialize(byte[] data, int length)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string jsonStr = Encoding.ASCII.GetString(data, 0, length);


            return JsonConvert.DeserializeObject<MessageBase>(jsonStr, settings);
        }

        #endregion
    }
}
