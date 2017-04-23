using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace WcfUtility
{
    public class WcfMessageLogger : IDispatchMessageInspector, IServiceBehavior, IEndpointBehavior, IClientMessageInspector
    {
        #region Service

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            WcfLogger.LogInfo("Servis aldı");
            WcfLogger.LogInfo(request.ToString());
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var msg = MessageToString(ref reply);
            using (StreamWriter sw = File.CreateText(@"C:\logs\service.txt"))
            {
                sw.WriteLine("Response to request to {0}:", (Uri)correlationState);
                HttpResponseMessageProperty httpResp = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                sw.WriteLine("{0} {1}", (int)httpResp.StatusCode, httpResp.StatusCode);

                if (!reply.IsEmpty)
                {
                    sw.WriteLine();
                    sw.WriteLine(this.MessageToString(ref reply));
                }
            }

            //string replyMsg = reply.ToString();
            //if (replyMsg.Contains("... stream ..."))
            //{
            //    MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
            //    reply = buffer.CreateMessage();

            //    //Hatayı logluyor
            //    Message copy = buffer.CreateMessage();
            //    XmlDictionaryReader bodyReader = copy.GetReaderAtBodyContents();
            //    bodyReader.ReadStartElement("Binary");
            //    byte[] bodyBytes = bodyReader.ReadContentAsBase64();
            //    replyMsg = Encoding.UTF8.GetString(bodyBytes);
            //}

            //WcfLogger.LogInfo("Servis dönüyor");
            //WcfLogger.LogInfo(msg);
        }

        private string MessageToString(ref Message message)
        {
            WebContentFormat messageFormat = this.GetMessageContentFormat(message);
            MemoryStream ms = new MemoryStream();
            XmlDictionaryWriter writer = null;
            switch (messageFormat)
            {
                case WebContentFormat.Default:
                case WebContentFormat.Xml:
                    writer = XmlDictionaryWriter.CreateTextWriter(ms);
                    break;
                case WebContentFormat.Json:
                    writer = JsonReaderWriterFactory.CreateJsonWriter(ms);
                    break;
                case WebContentFormat.Raw:
                    // special case for raw, easier implemented separately 
                    return this.ReadRawBody(ref message);
            }

            message.WriteMessage(writer);
            writer.Flush();
            string messageBody = Encoding.UTF8.GetString(ms.ToArray());

            // Here would be a good place to change the message body, if so desired. 

            // now that the message was read, it needs to be recreated. 
            ms.Position = 0;

            // if the message body was modified, needs to reencode it, as show below 
            // ms = new MemoryStream(Encoding.UTF8.GetBytes(messageBody)); 

            XmlDictionaryReader reader;
            if (messageFormat == WebContentFormat.Json)
            {
                reader = JsonReaderWriterFactory.CreateJsonReader(ms, XmlDictionaryReaderQuotas.Max);
            }
            else
            {
                reader = XmlDictionaryReader.CreateTextReader(ms, XmlDictionaryReaderQuotas.Max);
            }

            Message newMessage = Message.CreateMessage(reader, int.MaxValue, message.Version);
            newMessage.Properties.CopyProperties(message.Properties);
            message = newMessage;

            return messageBody;
        }

        private string ReadRawBody(ref Message message)
        {
            XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
            bodyReader.ReadStartElement("Binary");
            byte[] bodyBytes = bodyReader.ReadContentAsBase64();
            string messageBody = Encoding.UTF8.GetString(bodyBytes);

            // Now to recreate the message 
            MemoryStream ms = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(ms);
            writer.WriteStartElement("Binary");
            writer.WriteBase64(bodyBytes, 0, bodyBytes.Length);
            writer.WriteEndElement();
            writer.Flush();
            ms.Position = 0;
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max);
            Message newMessage = Message.CreateMessage(reader, int.MaxValue, message.Version);
            newMessage.Properties.CopyProperties(message.Properties);
            message = newMessage;

            return messageBody;
        }

        private WebContentFormat GetMessageContentFormat(Message message)
        {
            WebContentFormat format = WebContentFormat.Default;
            if (message.Properties.ContainsKey(WebBodyFormatMessageProperty.Name))
            {
                WebBodyFormatMessageProperty bodyFormat;
                bodyFormat = (WebBodyFormatMessageProperty)message.Properties[WebBodyFormatMessageProperty.Name];
                format = bodyFormat.Format;
            }

            return format;
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Method intentionally left empty.
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            // Method intentionally left empty.
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpoint in dispatcher.Endpoints)
                {
                    endpoint.DispatchRuntime.MessageInspectors.Add(new WcfMessageLogger());
                }
            }
        }

        #endregion Service

        #region Client

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            WcfLogger.LogInfo("Client gönderdi");
            WcfLogger.LogInfo(request.ToString());
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            WcfLogger.LogInfo("Client cevap aldı");
            WcfLogger.LogInfo(reply.ToString());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // Method intentionally left empty.
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // Method intentionally left empty.
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // Method intentionally left empty.
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new WcfMessageLogger());
        }

        #endregion Client
    }

    public class WcfMessageLoggerExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get
            {
                return typeof(WcfMessageLogger);
            }
        }

        protected override object CreateBehavior()
        {
            return new WcfMessageLogger();
        }
    }
}