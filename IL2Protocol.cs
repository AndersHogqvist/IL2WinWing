using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Runtime.InteropServices;

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
            public SIndicator(IndicatorID id = IndicatorID.UNKNOWN)
            {
                this.id = id;
            }
            public IndicatorID id { get; set; }
            public byte numOfValues { get; set; } = 0;
            public List<float> values = new List<float>();
        }

        public class STEEngineSetup
        {
            public short nIndex { get; set; } = 0;
            public short nID { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];
            public float fMaxRPM { get; set; } = 0.0f;
        }

        public class STEGunSetup
        {
            public short nIndex { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];
            public float fProjectileMass { get; set; } = 0.0f;
            public float fShootVelocity { get; set; } = 0.0f;
        }

        public class STELandingGearSetup
        {
            public short nIndex { get; set; } = 0;
            public short nID { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];
        }

        public class STEDropData
        {
            public float[] afPos { get; set; } = new float[3];
            public float fMass { get; set; } = 0.0f;
            public ushort uFlags { get; set; } = 0;
        }

        public class STEHit
        {
            public float[] afPos { get; set; } = new float[3];
            public float[] afHitF { get; set; } = new float[3];
        }

        public class STEExplosion
        {
            public float[] afPos { get; set; } = new float[3];
            public float fExpRad { get; set; } = 0.0f;
        }

        public class STClientData
        {
            public long nClientID { get; set; }
            public long nServerClientID { get; set; }
            public char[] sPlayerName { get; set; } = new char[32];
        }

        public class STControlledData
        {
            public long nParentClientID { get; set; } = 0;
            public short nCoalitionID { get; set; } = 0;
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
            public byte size { get; set; } = 0;

            public abstract string Print();

            public abstract byte[] Serialize();
        }

        public class SEventSetFocus : SEvent
        {
            public SEventSetFocus(string data = "") : base(EventID.SET_FOCUS)
            {
                size = (byte)data.Length;
                this.data = data;
            }
            public string data { get; set; }

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }
        }

        public class SEventSetupEngine : SEvent
        {
            public SEventSetupEngine() : base(EventID.SETUP_ENG)
            {
                size = sizeof(short) * 2 + sizeof(float) * 4;
            }
            public STEEngineSetup data { get; set; } = new STEEngineSetup();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.nIndex);
                        writer.Write(data.nID);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.fMaxRPM);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventSetupGun : SEvent
        {
            public SEventSetupGun() : base(EventID.SETUP_GUN)
            {
                size = sizeof(short) + sizeof(float) * 5;
            }
            public STEGunSetup data { get; set; } = new STEGunSetup();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.nIndex);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.fProjectileMass);
                        writer.Write(data.fShootVelocity);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventSetupLandingGear : SEvent
        {
            public SEventSetupLandingGear() : base(EventID.SETUP_LGEAR)
            {
                size = sizeof(short) * 2 + sizeof(float) * 3;
            }
            public STELandingGearSetup data { get; set; } = new STELandingGearSetup();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.nIndex);
                        writer.Write(data.nID);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventDropBomb : SEvent
        {
            public SEventDropBomb() : base(EventID.DROP_BOMB)
            {
                size = sizeof(float) * 4 + sizeof(ushort);
            }
            public STEDropData data { get; set; } = new STEDropData();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.fMass);
                        writer.Write(data.uFlags);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventRocketLaunch : SEvent
        {
            public SEventRocketLaunch() : base(EventID.ROCKET_LAUNCH)
            {
                size = sizeof(float) * 4 + sizeof(ushort);
            }
            public STEDropData data { get; set; } = new STEDropData();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.fMass);
                        writer.Write(data.uFlags);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventHit : SEvent
        {
            public SEventHit() : base(EventID.HIT)
            {
                size = sizeof(float) * 6;
            }
            public STEHit data { get; set; } = new STEHit();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.afHitF[0]);
                        writer.Write(data.afHitF[1]);
                        writer.Write(data.afHitF[2]);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventDamage : SEvent
        {
            public SEventDamage() : base(EventID.DAMAGE)
            {
                size = sizeof(float) * 6;
            }
            public STEHit data { get; set; } = new STEHit();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.afHitF[0]);
                        writer.Write(data.afHitF[1]);
                        writer.Write(data.afHitF[2]);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventExplosion : SEvent
        {
            public SEventExplosion() : base(EventID.EXPLOSION)
            {
                size = sizeof(float) * 4;
            }
            public STEExplosion data { get; set; } = new STEExplosion();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.afPos[0]);
                        writer.Write(data.afPos[1]);
                        writer.Write(data.afPos[2]);
                        writer.Write(data.fExpRad);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventGunFire : SEvent
        {
            public SEventGunFire() : base(EventID.GUN_FIRE)
            {
                size = 1;
            }
            public byte data { get; set; } = 0;

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                return new byte[] { (byte)id, size, data };
            }
        }

        public class SEventServerAddress : SEvent
        {
            public SEventServerAddress() : base(EventID.SRV_ADDR) { }
            public string data { get; set; } = "";

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }
        }

        public class SEventServerTitle : SEvent
        {
            public SEventServerTitle() : base(EventID.SRV_TITLE) { }
            public string data { get; set; } = "";

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }
        }

        public class SEventSRSAddress : SEvent
        {
            public SEventSRSAddress() : base(EventID.SRS_ADDR) { }
            public string data { get; set; } = "";

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }
        }

        public class SEventClientData : SEvent
        {
            public SEventClientData() : base(EventID.CLIENT_DAT) { }
            public STClientData data { get; set; } = new STClientData();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.nClientID);
                        writer.Write(data.nServerClientID);
                        writer.Write(data.sPlayerName);
                        return ms.ToArray();
                    }
                }
            }
        }

        public class SEventControlledData : SEvent
        {
            public SEventControlledData() : base(EventID.CTRL_DAT) { }
            public STControlledData data { get; set; } = new STControlledData();

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((short)id);
                        writer.Write(size);
                        writer.Write(data.nParentClientID);
                        writer.Write(data.nCoalitionID);
                        return ms.ToArray();
                    }
                }
            }
        }

        public enum MessageType : uint
        {
            TELEMETRY = 0x54000101,
            MOTION = 0x494C0100,
            UNKNOWN,
        }

        public static class MessageTypeExtensions
        {
            public static MessageType ToMessageType(this uint id)
            {
                return id == 0x54000101 ? MessageType.TELEMETRY
                     : id == 0x494C0100 ? MessageType.MOTION
                                        : MessageType.UNKNOWN;
            }
        }

        public abstract class BaseMessage
        {
            public MessageType type { get; set; } = MessageType.UNKNOWN;
            public ushort size { get; set; } = 0;
            public uint tick { get; set; } = 0;

            public BaseMessage(MessageType type)
            {
                this.type = type;
            }

            public abstract string Print();

            public abstract byte[] Serialize(ref string? debug);
        }

        public class Telemetry : BaseMessage
        {
            public Telemetry() : base(MessageType.TELEMETRY) { }

            public byte numOfIndicators { get; set; } = 0;
            public List<SIndicator> indicators { get; set; } = new List<SIndicator>();
            public byte numOfEvents { get; set; } = 0;
            public List<SEvent> events { get; set; } = new List<SEvent>();

            public override string Print()
            {
                if (size == 0)
                {
                    size = sizeof(byte) * 2 + ;
                }
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize(ref string? debug)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((uint)type);
                        writer.Write(size);
                        writer.Write(tick);
                        writer.Write(numOfIndicators);
                        foreach (SIndicator indicator in indicators)
                        {
                            writer.Write((ushort)indicator.id);
                            writer.Write(indicator.numOfValues);
                            foreach (float value in indicator.values)
                            {
                                writer.Write(value);
                            }
                        }
                        writer.Write(numOfEvents);
                        foreach (SEvent ev in events)
                        {
                            if (debug != null)
                            {
                                debug = $"Event {ev.id} size {ev.size}";
                            }
                            writer.Write(ev.Serialize());
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        public class Motion : BaseMessage
        {
            public Motion() : base(MessageType.MOTION) { }

            public float yaw { get; set; } = 0.0f; // [rad]
            public float pitch { get; set; } = 0.0f; // [rad]
            public float roll { get; set; } = 0.0f; // [rad]
            public float[] spin { get; set; } = new float[3]; // [rad/s]
            public float[] acc { get; set; } = new float[3]; // [m/s^2]

            public override string Print()
            {
                return $"{JsonSerializer.Serialize(this)}\n";
            }

            public override byte[] Serialize(ref string? debug)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((uint)type);
                        writer.Write(size);
                        writer.Write(tick);
                        writer.Write(yaw);
                        writer.Write(pitch);
                        writer.Write(roll);
                        writer.Write(spin[0]);
                        writer.Write(spin[1]);
                        writer.Write(spin[2]);
                        writer.Write(acc[0]);
                        writer.Write(acc[1]);
                        writer.Write(acc[2]);
                        return ms.ToArray();
                    }
                }
            }
        }
    } // namespace IL2Protocol
} // namespace IL2WinWing
