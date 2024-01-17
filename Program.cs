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
        private UdpClient telemetryClient = new UdpClient(Properties.Settings.Default.IL2TelemetryPort);
        private UdpClient motionClient = new UdpClient(Properties.Settings.Default.IL2MotionPort);
        private IPEndPoint teleEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Properties.Settings.Default.IL2TelemetryPort);
        private IPEndPoint motionEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Properties.Settings.Default.IL2MotionPort);

        private readonly NotifyIcon trayIcon;
        private DebugWindow debugWindow;
        private bool run = true;

        private WWAPI wwAPI = new WWAPI();

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

            wwAPI.StartListen();
        }

        ~CustomAppContext()
        {
            wwAPI.StopListen();
            run = false;
        }

        private void ShowDebugWindow(object sender, EventArgs e)
        {
            debugWindow = new DebugWindow();
            debugWindow.Show();

            //try
            //{
            //    string? debug = "";
            //    byte[] data = DummyData.GetTelemetry(ref debug);
            //    if (data != null)
            //    {
            //        debugWindow.AddText(debug);
            //    }
            //    client.Send(data, data.Length, groupEP);
            //}
            //catch (SocketException ex)
            //{
            //    Console.WriteLine(ex);
            //}
        }

        private void IL2Listener()
        {
            try
            {
                while (run)
                {
                    byte[] bytes = telemetryClient.Receive(ref teleEP);

                    if (bytes.Length > 0)
                    {
                        ParseIL2Message(bytes);
                    }

                    bytes = motionClient.Receive(ref motionEP);

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
                telemetryClient.Close();
            }
        }

        private void ParseIL2Message(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var reader = new BinaryReader(ms))
            {
                var type = reader.ReadUInt32();

                if (type == (UInt32)IL2Protocol.MessageType.TELEMETRY)
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
                                    var data = reader.ReadBytes((int)size);
                                    ev.data = Encoding.ASCII.GetString(data);
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
                                    var data = reader.ReadBytes((int)size);
                                    ev.data = Encoding.UTF8.GetString(data);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SRV_TITLE:
                                {
                                    var ev = new IL2Protocol.SEventServerTitle();
                                    ev.id = id;
                                    ev.size = size;
                                    var data = reader.ReadBytes((int)size);
                                    ev.data = Encoding.ASCII.GetString(data);
                                    telemetry.events.Add(ev);
                                    break;
                                }
                            case IL2Protocol.EventID.SRS_ADDR:
                                {
                                    var ev = new IL2Protocol.SEventSRSAddress();
                                    ev.id = id;
                                    ev.size = size;
                                    var data = reader.ReadBytes((int)size);
                                    ev.data = Encoding.ASCII.GetString(data);
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


                    if (debugWindow != null && debugWindow.printText)
                    {
                        debugWindow.AddText(telemetry.ToString());
                    }

                    var aoa = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.AOA);
                    var eas = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.EAS);

                    if (aoa != null && eas != null)
                    {
                        WWAPI.WWTelemetryMsg wwTelemetry = new WWAPI.WWTelemetryMsg();
                        wwTelemetry.args.angleOfAttack = aoa.values[0] * (180.0F / float.Pi);
                        wwTelemetry.args.trueAirSpeed = eas.values[0] * 9.0F;
                        if (!wwAPI.Send(WWAPI.WWMessage.UPDATE, wwTelemetry) && debugWindow != null)
                        {
                            debugWindow.AddText("Failed to send WW telemetry");
                        }
                    }
                    else
                    {
                        wwAPI.Send(WWAPI.WWMessage.STOP);
                    }
                }
                else if (type == (uint)IL2Protocol.MessageType.MOTION_ID)
                {
                    IL2Protocol.Motion motion= new IL2Protocol.Motion();
                    motion.tick = reader.ReadUInt32();
                    motion.yaw = reader.ReadSingle();
                    motion.pitch = reader.ReadSingle();
                    motion.roll = reader.ReadSingle();
                    motion.spin[0] = reader.ReadSingle();
                    motion.spin[1] = reader.ReadSingle();
                    motion.spin[2] = reader.ReadSingle();
                    motion.acc[0] = reader.ReadSingle();
                    motion.acc[1] = reader.ReadSingle();
                    motion.acc[2] = reader.ReadSingle();

                    if (debugWindow != null && debugWindow.printText)
                    {
                        debugWindow.AddText(motion.ToString());
                    }
                }
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            wwAPI.StopListen();
            run = false;
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}