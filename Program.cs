using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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

        private IL2Protocol.Motion motion = new IL2Protocol.Motion();
        private int gunShells = 1000;

        private readonly NotifyIcon trayIcon;
        private DebugWindow? debugWindow;
        private bool run = true;
        private bool waitingForWWInit = false;

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
            wwAPI.WWMessageReceived += OnWWMessage;
            wwAPI.StartListen();
        }

        ~CustomAppContext()
        {
            wwAPI.TearDown();
            run = false;
        }

        private void OnWWMessage(object? sender, WWMessageEventArgs e)
        {
            debugWindow?.AddText("FROM WW: " + e.msg);
            if (e.msg.Contains("addCommon"))
            {
                wwAPI.wwInit = true;
                waitingForWWInit = false;
            }
        }

        private void ShowDebugWindow(object? sender, EventArgs e)
        {
            debugWindow = new DebugWindow();
            debugWindow.Show();
        }

        private void IL2Listener()
        {
            byte[] bytes;
            try
            {
                while (run)
                {
                    bytes = telemetryClient.Receive(ref teleEP);

                    if (bytes.Length > 0)
                    {
                        if (!wwAPI.wwInit && !waitingForWWInit)
                        {
                            waitingForWWInit = true;
                            wwAPI.Send(WWAPI.WWMessage.START);
                        }
                        else
                        {
                            ParseIL2Message(bytes);
                        }
                    }

                    //bytes = motionClient.Receive(ref motionEP);

                    //if (bytes.Length > 0)
                    //{
                    //    if (!wwAPI.wwInit && !waitingForWWInit)
                    //    {
                    //        waitingForWWInit = true;
                    //        wwAPI.Send(WWAPI.WWMessage.START);
                    //    }
                    //    else
                    //    {
                    //        ParseIL2Message(bytes);
                    //    }
                    //}

                    Thread.Sleep(25);
                }
            }
            catch (SocketException e)
            {
                debugWindow?.AddText(e.Message);
            }
            finally
            {
                run = false;
                telemetryClient.Close();
                motionClient.Close();
            }
        }

        private void ParseIL2Message(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);
            var type = reader.ReadUInt32();


            if (type == (uint)IL2Protocol.MessageType.TELEMETRY)
            {
                IL2Protocol.Telemetry telemetry = new IL2Protocol.Telemetry();
                telemetry = new IL2Protocol.Telemetry
                {
                    size = reader.ReadUInt16(),
                    tick = reader.ReadUInt32(),
                    numOfIndicators = reader.ReadByte()
                };

                for (int ix = 0; ix < (int)telemetry.numOfIndicators; ix++)
                {

                    IL2Protocol.SIndicator ind = new IL2Protocol.SIndicator
                    {
                        id = IL2Protocol.IndicatorIDExtensions.ToIndicatorID(reader.ReadUInt16()),
                        numOfValues = reader.ReadByte()
                    };
                    ind.values = new float[ind.numOfValues];
                    for (int ix2 = 0; ix2 < (int)ind.numOfValues; ix2++)
                    {
                        ind.values[ix2] = reader.ReadSingle();
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
                                var ev = new IL2Protocol.SEventSetFocus
                                {
                                    id = id,
                                    size = size
                                };
                                ev.data = reader.ReadString();
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.SETUP_ENG:
                            {
                                var ev = new IL2Protocol.SEventSetupEngine
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventSetupGun
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventSetupLandingGear
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventDropBomb
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventRocketLaunch
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventHit
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventDamage
                                {
                                    id = id,
                                    size = size
                                };
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
                                var ev = new IL2Protocol.SEventExplosion
                                {
                                    id = id,
                                    size = size
                                };
                                ev.data.afPos[0] = reader.ReadSingle();
                                ev.data.afPos[1] = reader.ReadSingle();
                                ev.data.afPos[2] = reader.ReadSingle();
                                ev.data.fExpRad = reader.ReadSingle();
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.GUN_FIRE:
                            {
                                var ev = new IL2Protocol.SEventGunFire
                                {
                                    id = id,
                                    size = size,
                                    data = reader.ReadByte()
                                };
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.SRV_ADDR:
                            {
                                var ev = new IL2Protocol.SEventServerAddress
                                {
                                    id = id,
                                    size = size
                                };
                                var strLength = reader.ReadByte();
                                var str = reader.ReadBytes((int)strLength);
                                ev.data = Encoding.ASCII.GetString(str);
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.SRV_TITLE:
                            {
                                var ev = new IL2Protocol.SEventServerTitle
                                {
                                    id = id,
                                    size = size
                                };
                                ev.data = reader.ReadString();
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.SRS_ADDR:
                            {
                                var ev = new IL2Protocol.SEventSRSAddress
                                {
                                    id = id,
                                    size = size
                                };
                                ev.data = reader.ReadString();
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.CLIENT_DAT:
                            {
                                var ev = new IL2Protocol.SEventClientData
                                {
                                    id = id,
                                    size = size
                                };
                                ev.data.nClientID = reader.ReadInt64();
                                ev.data.nServerClientID = reader.ReadInt64();
                                ev.data.sPlayerName = reader.ReadChars(32);
                                telemetry.events.Add(ev);
                                break;
                            }
                        case IL2Protocol.EventID.CTRL_DAT:
                            {
                                var ev = new IL2Protocol.SEventControlledData
                                {
                                    id = id,
                                    size = size
                                };
                                ev.data.nParentClientID = reader.ReadInt64();
                                ev.data.nCoalitionID = reader.ReadInt16();
                                telemetry.events.Add(ev);
                                break;
                            }
                        default:
                            break;
                    }
                }


                if (debugWindow != null && debugWindow.printText)
                {
                    debugWindow.AddText(telemetry.ToString());
                }

                if (telemetry.numOfIndicators == 0 && wwAPI.wwInit)
                {
                    debugWindow?.AddText("No indicators, send empty telemetry to WW");
                    if (!wwAPI.Send(WWAPI.WWMessage.UPDATE, new WWAPI.WWTelemetryMsg()))
                    {
                        debugWindow?.AddText("Failed to send empty ww");
                    }
                    return;
                }

                var aoa = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.AOA);
                var eas = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.EAS);
                var agl = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.AGL);
                var spdBrk = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.AIR_BRAKES);
                var gear = telemetry.indicators.Find(x => x.id == IL2Protocol.IndicatorID.LGEARS_STATE);

                var gunFire = telemetry.events.Find(x => x.id == IL2Protocol.EventID.GUN_FIRE);
                var bombDrop = telemetry.events.Find(x => x.id == IL2Protocol.EventID.DROP_BOMB);
                var rocketLaunch = telemetry.events.Find(x => x.id == IL2Protocol.EventID.ROCKET_LAUNCH);
                var hit = telemetry.events.Find(x => x.id == IL2Protocol.EventID.HIT);

                var agl_ft = agl != null ? agl.values[0] * 3.3F : 0.0F;
                float ft_above_ground = (float)Math.Floor((double)agl_ft / 1000);

                WWAPI.WWTelemetryMsg wwTelemetry = new WWAPI.WWTelemetryMsg();

                wwTelemetry.args.angleOfAttack = aoa != null ? aoa.values[0] * (180.0F / float.Pi) : wwTelemetry.args.angleOfAttack;
                wwTelemetry.args.trueAirSpeed = eas != null ? eas.values[0] + 0.02F * ft_above_ground : wwTelemetry.args.trueAirSpeed;
                wwTelemetry.args.gearValue = gear != null ? gear.values[0] : wwTelemetry.args.gearValue;
                if (gunFire != null)
                {
                    gunShells -= 1;
                    if (gunShells < 0)
                    {
                        gunShells = 1000;
                    }
                    wwTelemetry.args.cannonShellsCount = gunShells;
                }
                if (bombDrop != null || rocketLaunch != null || hit != null)
                {
                    gunShells -= 1;
                    if (gunShells < 0)
                    {
                        gunShells = 1000;
                    }
                    wwTelemetry.args.cannonShellsCount = gunShells;
                }
                wwTelemetry.args.speedbrakesValue = spdBrk != null ? spdBrk.values[0] : wwTelemetry.args.speedbrakesValue;
                wwTelemetry.args.verticalVelocity = motion.pitch;
                if (!wwAPI.Send(WWAPI.WWMessage.UPDATE, wwTelemetry))
                {
                    debugWindow?.AddText("Failed to send WW telemetry");
                }
                else if (debugWindow != null && debugWindow.printText)
                {
                    debugWindow.AddText("TO WW: " + JsonSerializer.Serialize(wwTelemetry));
                }
            }
            else if (type == (uint)IL2Protocol.MessageType.MOTION_ID)
            {
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
                    debugWindow.AddText(JsonSerializer.Serialize(motion));
                }
            }
        }

        private void Exit(object? sender, EventArgs e)
        {
            debugWindow?.Close();
            wwAPI.TearDown();
            run = false;
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}