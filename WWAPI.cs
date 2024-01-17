using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IL2WinWing
{
    public class WWMessageEventArgs : EventArgs
    {
        public JsonObject msg { get; set; }
    }
    internal class WWAPI
    {
        private UdpClient wwClient = new UdpClient();
        
        private IPEndPoint wwEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Properties.Settings.Default.WWPort);
        private bool listen = false;
        private bool wwInit = false;

        private const string NET_READY = "{\"func\": \"net\", \"msg\": \"ready\"}";
        private const string MSN_READY = "{\"func\": \"mission\", \"msg\": \"ready\"}";
        private const string MSN_START = "{\"func\": \"mission\", \"msg\": \"start\"}";
        private const string MOD       = "{\"func\": \"mod\", \"msg\": \"TF-51D\"}";
        private const string MSN_STOP  = "{\"func\": \"mission\", \"msg\": \"stop\"}";

        public class Args
        {
            public float angleOfAttack { get; set; }
            public float trueAirSpeed { get; set; }
            public List<object> payloadStations { get; set; } = new List<object>();
        }

        public class WWTelemetryMsg
        {
            public string func { get; } = "addCommon";
            public Args args { get; set; } = new Args();
        }

        public WWAPI()
        {
            wwClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            wwClient.Client.Connect(wwEP);
        }
        ~WWAPI()
        {
            wwClient.Close();
        }
        
        public void StartListen()
        {
            listen = true;
            if (!wwInit)
            {
                Send(WWMessage.START);
            }
            Task.Run(() => Receiver());
        }

        public void StopListen()
        {
            Send(WWMessage.STOP);
            listen = false;
            wwInit = false;
            wwClient.Close();
        }

        public enum WWMessage
        {
            START,
            UPDATE,
            STOP,
        }

        public bool Send(WWMessage msg, WWTelemetryMsg? telemetry = null)
        {
            if (msg == WWMessage.START)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(MSN_STOP);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    // do nothing
                }
                Thread.Sleep(30);
                bytes = Encoding.ASCII.GetBytes(NET_READY);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    return false;
                }
                Thread.Sleep(30);
                bytes = Encoding.ASCII.GetBytes(MSN_READY);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    return false;
                }
                Thread.Sleep(30);
                bytes = Encoding.ASCII.GetBytes(MSN_START);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    return false;
                }
                Thread.Sleep(30);
                bytes = Encoding.ASCII.GetBytes(MOD);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    return false;
                }

                Thread.Sleep(2000);
                wwInit = true;
            }
            else if (msg == WWMessage.UPDATE && telemetry != null && wwInit)
            {
                string json = JsonSerializer.Serialize(telemetry);
                byte[] bytes = Encoding.ASCII.GetBytes(json);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (msg == WWMessage.STOP)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(MSN_STOP);
                try
                {
                    wwClient.Send(bytes, bytes.Length, wwEP);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
            return true;
        }

        private void Receiver()
        {
            while (listen)
            {
                try
                {
                    byte[] data = wwClient.Receive(ref wwEP);

                    if (data.Length > 0)
                    {
                        string json = Encoding.ASCII.GetString(data);
                        JsonObject obj = JsonNode.Parse(json) as JsonObject;
                        WWMessageEventArgs msg = new WWMessageEventArgs();
                        msg.msg = obj;
                    }
                }
                catch (Exception)
                {
                    listen = false;
                }

                Thread.Sleep(30);
            }
        }

        protected virtual void OnWWMessage(WWMessageEventArgs e)
        {
            EventHandler<WWMessageEventArgs> handler = WWMessageReceived;
            handler?.Invoke(this, e);
        }

        public event EventHandler<WWMessageEventArgs> WWMessageReceived;
    }
}
