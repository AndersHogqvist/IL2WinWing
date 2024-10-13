using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IL2WinWing
{
    namespace IL2Protocol
    {
        public enum IndicatorID : ushort
        {
            ENG_RPM,
            ENG_MP,
            ENG_SHAKE_FRQ,
            ENG_SHAKE_AMP,
            LGEARS_STATE,
            LGEARS_PRESS,
            EAS,
            AOA,
            ACCELERATION,
            COCKPIT_SHAKE,
            AGL,
            FLAPS,
            AIR_BRAKES,
            UNKNOWN,
        };

        public static class IndicatorIDExtensions
        {
            public static IndicatorID ToIndicatorID(this ushort id)
            {
                return id < 13 ? (IndicatorID)id : IndicatorID.UNKNOWN;
            }
        }

        public class SIndicator
        {
            public IndicatorID id { get; set; }
            public byte numOfValues { get; set; } = 0;
            public float[]? values { get; set; }
            public SIndicator(IndicatorID id = IndicatorID.UNKNOWN)
            {
                this.id = id;
            }

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STEEngineSetup
        {
            public short nIndex { get; set; } = 0;
            public short nID { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];
            public float fMaxRPM { get; set; } = 0.0f;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STEGunSetup
        {
            public short nIndex { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];
            public float fProjectileMass { get; set; } = 0.0f;
            public float fShootVelocity { get; set; } = 0.0f;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STELandingGearSetup
        {
            public short nIndex { get; set; } = 0;
            public short nID { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STEDropData
        {
            public float[] afPos { get; set; } = new float[3];
            public float fMass { get; set; } = 0.0f;
            public ushort uFlags { get; set; } = 0;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STEHit
        {
            public float[] afPos { get; set; } = new float[3];
            public float[] afHitF { get; set; } = new float[3];

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STEExplosion
        {
            public float[] afPos { get; set; } = new float[3];
            public float fExpRad { get; set; } = 0.0f;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STClientData
        {
            public int nClientID { get; set; }
            public int nServerClientID { get; set; }
            public char[] sPlayerName { get; set; } = new char[32];

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class STControlledData
        {
            public int nParentClientID { get; set; } = 0;
            public short nCoalitionID { get; set; } = 0;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public enum EventID : short
        {
            SET_FOCUS,
            SETUP_ENG,
            SETUP_GUN,
            SETUP_LGEAR,
            DROP_BOMB,
            ROCKET_LAUNCH,
            HIT,
            DAMAGE,
            EXPLOSION,
            GUN_FIRE,
            SRV_ADDR,
            SRV_TITLE,
            SRS_ADDR,
            CLIENT_DAT,
            CTRL_DAT,
            UNKNOWN,
        };

        public static class EventIDExtensions
        {
            public static EventID ToEventID(this ushort id)
            {
                return id < 15 ? (EventID)id : EventID.UNKNOWN;
            }
        }

        public abstract class SEvent
        {
            public SEvent(EventID id = EventID.UNKNOWN)
            {
                this.id = id;
            }
            public EventID id { get; set; }
            public byte? size { get; set; }
        }

        public class SEventSetFocus : SEvent
        {
            public SEventSetFocus() : base(EventID.SET_FOCUS)
            {
            }
            public string? data { get; set; }

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventSetupEngine : SEvent
        {
            public SEventSetupEngine() : base(EventID.SETUP_ENG)
            {
            }
            public STEEngineSetup data { get; set; } = new STEEngineSetup();
            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventSetupGun : SEvent
        {
            public SEventSetupGun() : base(EventID.SETUP_GUN)
            {
            }
            public STEGunSetup data { get; set; } = new STEGunSetup();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventSetupLandingGear : SEvent
        {
            public SEventSetupLandingGear() : base(EventID.SETUP_LGEAR)
            {
            }
            public STELandingGearSetup data { get; set; } = new STELandingGearSetup();
            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventDropBomb : SEvent
        {
            public SEventDropBomb() : base(EventID.DROP_BOMB)
            {
            }
            public STEDropData data { get; set; } = new STEDropData();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventRocketLaunch : SEvent
        {
            public SEventRocketLaunch() : base(EventID.ROCKET_LAUNCH)
            {
            }
            public STEDropData data { get; set; } = new STEDropData();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventHit : SEvent
        {
            public SEventHit() : base(EventID.HIT)
            {
            }
            public STEHit data { get; set; } = new STEHit();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventDamage : SEvent
        {
            public SEventDamage() : base(EventID.DAMAGE)
            {
            }
            public STEHit data { get; set; } = new STEHit();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventExplosion : SEvent
        {
            public SEventExplosion() : base(EventID.EXPLOSION)
            {
            }
            public STEExplosion data { get; set; } = new STEExplosion();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventGunFire : SEvent
        {
            public SEventGunFire() : base(EventID.GUN_FIRE)
            {
            }
            public byte data { get; set; } = 0;

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventServerAddress : SEvent
        {
            public SEventServerAddress() : base(EventID.SRV_ADDR) { }
            public string data { get; set; } = "";

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventServerTitle : SEvent
        {
            public SEventServerTitle() : base(EventID.SRV_TITLE) { }
            public string data { get; set; } = "";

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventSRSAddress : SEvent
        {
            public SEventSRSAddress() : base(EventID.SRS_ADDR) { }
            public string data { get; set; } = "";
            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventClientData : SEvent
        {
            public SEventClientData() : base(EventID.CLIENT_DAT) { }
            public STClientData data { get; set; } = new STClientData();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public class SEventControlledData : SEvent
        {
            public SEventControlledData() : base(EventID.CTRL_DAT) { }
            public STControlledData data { get; set; } = new STControlledData();

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }

        public enum MessageType : uint
        {
            TELEMETRY = 0x54000101,
            MOTION_ID = 0x494C0100,
        }

        public class Telemetry
        {
            public uint id { get; } = (uint)MessageType.TELEMETRY;
            public ushort size { get; set; } = 0;
            public uint tick { get; set; } = 0;
            public byte numOfIndicators { get; set; } = 0;
            public List<SIndicator> indicators { get; set; } = [];
            public byte numOfEvents { get; set; } = 0;
            public List<SEvent> events { get; set; } = [];

            public override string ToString()
            {
                var str = $"{{id: {id.ToString()}, size: {size}, tick: {tick}, numOfIndicators: {(int)numOfIndicators}, indicators: [";
                foreach (var indicator in indicators)
                {
                    str += indicator.ToString() + ", ";
                }
                str = str.TrimEnd(',', ' ');
                str += $"], numOfEvents: {(int)numOfEvents}, events: [";
                foreach (var ev in events)
                {
                    str += ev.ToString() + ", ";
                }
                str = str.TrimEnd(',', ' ');
                str += "]}";
                return str;
            }
        }

        public class Motion
        {
            public uint id { get; } = (uint)MessageType.MOTION_ID;
            public uint tick { get; set; } = 0;
            public float yaw { get; set; } = 0.0f; // [rad]
            public float pitch { get; set; } = 0.0f; // [rad]
            public float roll { get; set; } = 0.0f; // [rad]
            public float[] spin { get; set; } = new float[3]; // [rad/s]
            public float[] acc { get; set; } = new float[3]; // [m/s^2]

            public override string ToString()
            {
                return JsonSerializer.Serialize(this);
            }
        }
    } // namespace IL2Protocol
} // namespace IL2WinWing
