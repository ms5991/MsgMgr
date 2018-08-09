using Newtonsoft.Json;
using System;
using System.Text;
using Newtonsoft.Json.Serialization;
using MsgMgr.Serialization;

namespace MsgMgr.Core
{
    public abstract class MessageBase : SerializableBase
    {
        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        [SerializableProperty]
        public Guid Identity { get; private set; }

        /// <summary>
        /// Gets or sets the time sent.
        /// </summary>
        /// <value>
        /// The time sent.
        /// </value>
        [SerializableProperty]
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Gets or sets the time received.
        /// </summary>
        /// <value>
        /// The time received.
        /// </value>
        [SerializableProperty]
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
    }
}
