using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IL2WinWing
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();
            Application.Run(new CustomAppContext());
        }
    }

    public class CustomAppContext : ApplicationContext
    {
        private UdpClient client = new UdpClient(Properties.Settings.Default.IL2Port);
        private IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Properties.Settings.Default.IL2Port);

        private readonly NotifyIcon trayIcon;
        private DebugWindow debugWindow;
        private bool run = true;
        private bool debug = false;

        public CustomAppContext()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            trayIcon.Text = "IL2WinWing";
            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add("Debug", null, ShowDebugWindow);
            trayIcon.ContextMenuStrip.Items.Add("-");
            trayIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);
            trayIcon.Visible = true;

            new Task(IL2Listener).Start();
        }

        ~CustomAppContext()
        {
            run = false;
        }

        private void DebugWindow_FormClosing(object sender, EventArgs e)
        {
            debug = false;
            debugWindow.ClearText();
        }

        private void ShowDebugWindow(object sender, EventArgs e)
        {
            debugWindow = new DebugWindow();
            debugWindow.FormClosing += DebugWindow_FormClosing;

            debug = true;
            debugWindow.Show();
            try
            {
                string? debug = "";
                byte[] data = DummyData.GetTelemetry(ref debug);
                if (data != null)
                {
                    debugWindow.AddText(debug);
                }
                client.Send(data, data.Length, groupEP);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void IL2Listener()
        {
            try
            {
                while (run)
                {
                    byte[] bytes = client.Receive(ref groupEP);

                    if (bytes.Length > 0)
                    {
                        ParseIL2Message(bytes);
                    }

                    Thread.Sleep(30);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                run = false;
                client.Close();
            }
        }

        private void ParseIL2Message(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var reader = new BinaryReader(ms))
            {
                var type = IL2Protocol.MessageTypeExtensions.ToMessageType(reader.ReadUInt32());

                if (type == IL2Protocol.MessageType.TELEMETRY)
                {
                    IL2Protocol.Telemetry telemetry = new IL2Protocol.Telemetry();
                    telemetry.size = reader.ReadUInt16();
                    telemetry.tick = reader.ReadUInt32();
                    telemetry.numOfIndicators = reader.ReadByte();

                    for (int ix = 0; ix < (int)telemetry.numOfIndicators; ix++)
                    {
                        IL2Protocol.SIndicator ind = new IL2Protocol.SIndicator();
                        ind.id = IL2Protocol.IndicatorIDExtensions.ToIndicatorID(reader.ReadUInt16());
                        ind.numOfValues = reader.ReadByte();
                        for (int ix2 = 0; ix2 < (int)ind.numOfValues; ix2++)
                        {
                            ind.values.Add(reader.ReadSingle());
                        }

                        telemetry.indicators.Add(ind);
                    }

                    telemetry.numOfEvents = reader.ReadByte();

                    for (int ix = 0; ix < (int)telemetry.numOfEvents; ix++)
                    {
                        var id = IL2Protocol.EventIDExtensions.ToEventID(reader.ReadUInt16());
                        var size = reader.ReadByte();

                        switch (id)
                        {
                            case IL2Protocol.EventID.SET_FOCUS:
                                {
                                    var ev = new IL2Protocol.SEventSetFocus();
                                    ev.id = id;
                                    ev.size = size;
                                    var data = reader.ReadBytes(size);
                                    ev.data = Encoding.UTF8.GetString(data);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SETUP_ENG:
                                {
                                    var ev = new IL2Protocol.SEventSetupEngine();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.nIndex = reader.ReadInt16();
                                    ev.data.nID = reader.ReadInt16();
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.fMaxRPM = reader.ReadSingle();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SETUP_GUN:
                                {
                                    var ev = new IL2Protocol.SEventSetupGun();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.nIndex = reader.ReadInt16();
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.fProjectileMass = reader.ReadSingle();
                                    ev.data.fShootVelocity = reader.ReadSingle();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SETUP_LGEAR:
                                {
                                    var ev = new IL2Protocol.SEventSetupLandingGear();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.nIndex = reader.ReadInt16();
                                    ev.data.nID = reader.ReadInt16();
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.DROP_BOMB:
                                {
                                    var ev = new IL2Protocol.SEventDropBomb();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.fMass = reader.ReadSingle();
                                    ev.data.uFlags = reader.ReadUInt16();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.ROCKET_LAUNCH:
                                {
                                    var ev = new IL2Protocol.SEventRocketLaunch();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.fMass = reader.ReadSingle();
                                    ev.data.uFlags = reader.ReadUInt16();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.HIT:
                                {
                                    var ev = new IL2Protocol.SEventHit();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.afHitF[0] = reader.ReadSingle();
                                    ev.data.afHitF[1] = reader.ReadSingle();
                                    ev.data.afHitF[2] = reader.ReadSingle();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.DAMAGE:
                                {
                                    var ev = new IL2Protocol.SEventDamage();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.afHitF[0] = reader.ReadSingle();
                                    ev.data.afHitF[1] = reader.ReadSingle();
                                    ev.data.afHitF[2] = reader.ReadSingle();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.EXPLOSION:
                                {
                                    var ev = new IL2Protocol.SEventExplosion();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.afPos[0] = reader.ReadSingle();
                                    ev.data.afPos[1] = reader.ReadSingle();
                                    ev.data.afPos[2] = reader.ReadSingle();
                                    ev.data.fExpRad = reader.ReadSingle();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.GUN_FIRE:
                                {
                                    var ev = new IL2Protocol.SEventGunFire();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data = reader.ReadByte();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SRV_ADDR:
                                {
                                    var ev = new IL2Protocol.SEventServerAddress();
                                    ev.id = id;
                                    ev.size = size;
                                    var data = reader.ReadBytes(size);
                                    ev.data = Encoding.UTF8.GetString(data);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SRV_TITLE:
                                {
                                    var ev = new IL2Protocol.SEventServerTitle();
                                    ev.id = id;
                                    ev.size = size;
                                    var data = reader.ReadBytes(size);
                                    ev.data = Encoding.UTF8.GetString(data);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SRS_ADDR:
                                {
                                    var ev = new IL2Protocol.SEventSRSAddress();
                                    ev.id = id;
                                    ev.size = size;
                                    var data = reader.ReadBytes(size);
                                    ev.data = Encoding.UTF8.GetString(data);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.CLIENT_DAT:
                                {
                                    var ev = new IL2Protocol.SEventClientData();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.nClientID = reader.ReadInt64();
                                    ev.data.nServerClientID = reader.ReadInt64();
                                    ev.data.sPlayerName = reader.ReadChars(32);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.CTRL_DAT:
                                {
                                    var ev = new IL2Protocol.SEventControlledData();
                                    ev.id = id;
                                    ev.size = size;
                                    ev.data.nParentClientID = reader.ReadInt64();
                                    ev.data.nCoalitionID = reader.ReadInt16();
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            default:
                                break;
                        }
                        _ = reader.ReadBytes(2); // padding
                    }

                    if (debug)
                    {
                        debugWindow.AddText(telemetry.Print());
                    }
                }
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            run = false;
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}