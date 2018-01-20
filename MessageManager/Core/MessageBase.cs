using Newtonsoft.Json;
using System;
using System.Text;
using Newtonsoft.Json.Serialization;

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
        [JsonProperty]
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Gets or sets the time received.
        /// </summary>
        /// <value>
        /// The time received.
        /// </value>
        [JsonProperty]
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

        private static JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        /// <summary>
        /// Serializes the specified Message to JSON, then encodes that as ASCII.
        /// </summary>
        /// <param name="toSerialize">To serialize.</param>
        /// <returns></returns>
        internal static byte[] SerializeToBytes(MessageBase toSerialize)
        {
            string jsonStr = JsonConvert.SerializeObject(toSerialize, _settings);

            return Encoding.ASCII.GetBytes(jsonStr);
        }
        
        /// <summary>
        /// Deserializes the data from bytes to MessageBase.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        internal static MessageBase DeserializeFromBytes(byte[] data, int length)
        {
            string jsonStr = Encoding.ASCII.GetString(data, 0, length);

            return JsonConvert.DeserializeObject<MessageBase>(jsonStr, _settings);
        }

        /// <summary>
        /// Serializes the specified Message to json string.
        /// </summary>
        /// <param name="toSerialize">To serialize.</param>
        /// <returns></returns>
        internal static string SerializeToJsonString(MessageBase toSerialize)
        {
            return JsonConvert.SerializeObject(toSerialize, _settings);
        }
        
        /// <summary>
        /// Deserializes the data from json string to MessageBase.
        /// </summary>
        /// <param name="jsonStr">The json string.</param>
        /// <returns></returns>
        internal static MessageBase DeserializeFromJsonString(string jsonStr)
        {
            return JsonConvert.DeserializeObject<MessageBase>(jsonStr, _settings);
        }

        #endregion
    }
}
