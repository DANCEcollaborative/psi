
namespace CMU.Smartlab.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetMQ;
    using NetMQ.Sockets;
    using Microsoft.Psi;
    using Microsoft.Psi.Components;
    using Microsoft.Psi.Interop.Serialization;
    using Microsoft.Psi.Interop.Transport;
    using Apache.NMS;
    using Apache.NMS.ActiveMQ;
    using Apache.NMS.ActiveMQ.Transport.Discovery;

// public class AMQSubscriber 
    public class AMQSubscriber<T>: IProducer<T>, ISourceComponent, IDisposable
    {
        private readonly Pipeline pipeline;
        private readonly string name;
        private int port = 61616; 
        public bool Occupied = false;

        private IConnectionFactory factory = null;
        private IConnection connection = null;
        private ISession session = null;
    
        private string uri;
        private string activeMQUri;
        private Dictionary<string, IMessageProducer> producerList
            = new Dictionary<string, IMessageProducer>();

        private Dictionary<string, IMessageConsumer> consumerList
            = new Dictionary<string, IMessageConsumer>();
        private string inTopic;
        private string outTopic;
        private string clientID;
        private readonly bool useSourceOriginatingTimes;
        // private Envelope envelope;

        public AMQSubscriber(Pipeline pipeline, string inTopic, string outTopic, string clientID, bool useSourceOriginatingTimes = true, string name = nameof(AMQSubscriber<T>))
        {
            this.inTopic = inTopic;
            this.outTopic = outTopic; 
            Console.WriteLine("AMQSubscriber constructor - inTopic: '{0}'  --  outTopic: '{1}'", this.inTopic, this.outTopic);
            this.pipeline = pipeline;
            this.name = name;
            this.useSourceOriginatingTimes = false; 
            this.uri = string.Format("tcp://localhost:{0}", this.port);
            this.activeMQUri = uri;
            this.clientID = clientID;
            // this.StringIn = pipeline.CreateReceiver<string>(this, ReceiveString, nameof(this.StringIn));
            this.Out = pipeline.CreateEmitter<T>(this, outTopic);
            // this.envelope = pipeline.Envelope;
            // subscribe(inTopic,ProcessText);
        }


        // Receiver that encapsulates the string input stream
        public Receiver<string> StringIn { get; }

        public Emitter<string> StringOut { get; }

        // Emitter that encapsulates the output stream
        // public Emitter<string> Out { get; private set; }
        public Emitter<T> Out { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Stop();
        }

        /// <inheritdoc/>
        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted)
        {
            this.Stop();
            notifyCompleted();
        }

        private void Stop()
        {
            // if (this.socket != null)
            // {
            //     this.poller.Dispose();
            //     this.socket.Dispose();
            //     this.socket = null;
            // }
        }

        public void Start(Action<DateTime> notifyCompletionTime)
        {
            Console.WriteLine("AMQSubscriber Start - enter");
            // notify that this is an infinite source component
            notifyCompletionTime(DateTime.MaxValue);
            Console.WriteLine("AMQSubscriber.Start - InTopic:  '{0}'", this.inTopic);
            Console.WriteLine("AMQSubscriber.Start - OutTopic: '{0}'", this.outTopic);
            // InitActiveMQServer();
            this.factory = new NMSConnectionFactory(this.activeMQUri);
            try
            {
                this.connection = this.factory.CreateConnection();
                this.connection.ClientId = this.clientID;
                this.connection.Start();
                this.session = this.connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            subscribe(inTopic,outputString);
        }

        private IMessageProducer GetProducer(string topicName)
        {
            IMessageProducer producer;
            if (!this.producerList.TryGetValue(topicName, out producer))
            {
                IDestination destination = new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(topicName);
                producer = session.CreateProducer(destination);
                this.producerList.Add(topicName, producer);
            }

            return producer;
        }

        public void subscribe(string topic, Action<IMessage> listener)
        {
            IMessageConsumer consumer = this.GetConsumer(topic);
            consumer.Listener += new MessageListener(listener);
        }

        public void subscribe(string topic, Action<ITextMessage> listener)
        {
            Console.WriteLine("AMQSubscriber.cs: subscribe ITextMessage -- topic: " + topic);
            IMessageConsumer consumer = this.GetConsumer(topic);
            consumer.Listener += new MessageListener((message) =>
            {
                if (message is ITextMessage)
                {
                    ITextMessage textMessage = (ITextMessage)message;
                    Console.WriteLine("AMQSubscriber.cs: subscribe ITextMessage -- topic: " + topic + "  textMessage: " + textMessage);
                    listener.Invoke(textMessage);
                }
            });
        }

        // public void subscribe(string topic, Action<IBytesMessage> listener)
        // {
        //     IMessageConsumer consumer = this.GetConsumer(topic);
        //     consumer.Listener += new MessageListener((message) =>
        //     {
        //         if (message is IBytesMessage)
        //         {
        //             IBytesMessage bytesMessage = (IBytesMessage)message;
        //             listener.Invoke(bytesMessage);
        //         }
        //     });
        // }

        public void subscribe(string topic, Action<string> listener)
        {
            Console.WriteLine("AMQSubscriber.cs: subscribe string -- topic: " + topic);
            IMessageConsumer consumer = this.GetConsumer(topic);
            consumer.Listener += new MessageListener((message) =>
            {
                if (message is ITextMessage)
                {
                    string text = ((ITextMessage)message).Text;
                    Console.WriteLine("AMQSubscriber.cs: subscribe string -- topic: " + topic + "  textMessage: " + text);
                    listener.Invoke(text);
                }
            });
        }

        // public void subscribe(string topic, Action<byte[]> listener)
        // {
        //     IMessageConsumer consumer = this.GetConsumer(topic);
        //     consumer.Listener += new MessageListener((message) =>
        //     {
        //         if (message is IBytesMessage)
        //         {
        //             IBytesMessage bytesMessage = (IBytesMessage)message;
        //             byte[] bytes = new byte[bytesMessage.BodyLength];
        //             ((IBytesMessage)message).ReadBytes(bytes);
        //             listener.Invoke(bytes);
        //         }
        //     });
        // }

        private IMessageConsumer GetConsumer(string topicName)
        {
            IMessageConsumer consumer;
            if (!this.consumerList.TryGetValue(topicName, out consumer))
            {
                ITopic topic = new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(topicName);
                consumer = this.session.CreateDurableConsumer(topic, "consumer for " + topicName, null, false);
                this.consumerList.Add(topicName, consumer);
            }

            return consumer;
        }

        // private static void ProcessText(String s)
        // {
        //     if (s != null)
        //     {
        //         Console.WriteLine($">>> Send MULTIMODAL message to VHT: multimodal:false;%;identity:someone;%;text:{s}");
        //         ReceiveString(s, envelope);
        //     }
        // }
        /// <inheritdoc />
        // public void Dispose() {}
        // {
        //     if (this.socket != null)
        //     {
        //         this.socket.Dispose();
        //         this.socket = null;
        //     }
        // }

        /// <inheritdoc/>
        public override string ToString() => this.name;

        private void outputString (string outString)
        {
            Console.WriteLine("AMQSubscriber.cs, outputString: outString: " + outString);
            T outType = GetValue<T>(outString); 
            outputType(outType);
            // this.Out.Post(outString, this.pipeline.GetCurrentTime());
        }

        public static T GetValue<T>(String value)
            {
            return (T)Convert.ChangeType(value, typeof(T));
            }

        private void outputType (T outString)
        {
            Console.WriteLine("AMQSubscriber.cs, outputTuype: sending -- outTopic: " + outTopic + "  content: " + outString);
            this.Out.Post(outString, this.pipeline.GetCurrentTime());
        }

        // The receive method for the StringIn receiver. This executes every time a message arrives on StringIn.
        private void ReceiveString(string input, Envelope envelope)
        {
            Console.WriteLine("AMQSubscriber.cs, ReceiveString: sending -- outTopic: " + outTopic + "  content: " + input);
            IMessageProducer producer = this.GetProducer(outTopic);
            ITextMessage message = producer.CreateTextMessage(input);
            producer.Send(message, MsgDeliveryMode.Persistent, MsgPriority.Normal, TimeSpan.MaxValue);
        }
    }
}
