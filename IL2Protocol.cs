using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            public override string ToString()
            {
                var str = $"{{id: {id.ToString()}, numOfValues: {numOfValues}, values: [";
                foreach (float value in values)
                {
                    str += $"{value}, ";
                }
                str = str.Remove(str.Length - 2);
                str += "]}";
                return str;
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

            public override string ToString()
            {
                return $"{{nIndex: {nIndex}, nID: {nID}, afPos: [{afPos[0]}, {afPos[1]}, {afPos[2]}], fMaxRPM: {fMaxRPM}}}";
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
                return $"{{nIndex: {nIndex}, afPos: [{afPos[0]}, {afPos[1]}, {afPos[2]}], fProjectileMass: {fProjectileMass}, fShootVelocity: {fShootVelocity}}}";
            }
        }

        public class STELandingGearSetup
        {
            public short nIndex { get; set; } = 0;
            public short nID { get; set; } = 0;
            public float[] afPos { get; set; } = new float[3];

            public override string ToString()
            {
                return $"{{nIndex: {nIndex}, nID: {nID}, afPos: [{afPos[0]}, {afPos[1]}, {afPos[2]}]}}";
            }
        }

        public class STEDropData
        {
            public float[] afPos { get; set; } = new float[3];
            public float fMass { get; set; } = 0.0f;
            public ushort uFlags { get; set; } = 0;

            public override string ToString()
            {
                return $"{{afPos: [{afPos[0]}, {afPos[1]}, {afPos[2]}], fMass: {fMass}, uFlags: {uFlags}}}";
            }
        }

        public class STEHit
        {
            public float[] afPos { get; set; } = new float[3];
            public float[] afHitF { get; set; } = new float[3];

            public override string ToString()
            {
                return $"{{afPos: [{afPos[0]}, {afPos[1]}, {afPos[2]}], afHitF: [{afHitF[0]}, {afHitF[1]}, {afHitF[2]}]}}";
            }
        }

        public class STEExplosion
        {
            public float[] afPos { get; set; } = new float[3];
            public float fExpRad { get; set; } = 0.0f;

            public override string ToString()
            {
                return $"{{afPos: [{afPos[0]}, {afPos[1]}, {afPos[2]}], fExpRad: {fExpRad}}}";
            }
        }

        public class STClientData
        {
            public long nClientID { get; set; }
            public long nServerClientID { get; set; }
            public char[] sPlayerName { get; set; } = new char[32];

            public override string ToString()
            {
                return $"{{nClientID: {nClientID}, nServerClientID: {nServerClientID}, sPlayerName: {new string(sPlayerName)}}}";
            }
        }

        public class STControlledData
        {
            public long nParentClientID { get; set; } = 0;
            public short nCoalitionID { get; set; } = 0;

            public override string ToString()
            {
                return $"{{nParentClientID: {nParentClientID}, nCoalitionID: {nCoalitionID}}}";
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
            public byte size { get; set; } = 0;
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

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {data}}}";
            }
        }

        public class SEventSetupEngine : SEvent
        {
            public SEventSetupEngine() : base(EventID.SETUP_ENG)
            {
                size = sizeof(short) * 2 + sizeof(float) * 4;
            }
            public STEEngineSetup data { get; set; } = new STEEngineSetup();
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
            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventSetupGun : SEvent
        {
            public SEventSetupGun() : base(EventID.SETUP_GUN)
            {
                size = sizeof(short) + sizeof(float) * 5;
            }
            public STEGunSetup data { get; set; } = new STEGunSetup();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventSetupLandingGear : SEvent
        {
            public SEventSetupLandingGear() : base(EventID.SETUP_LGEAR)
            {
                size = sizeof(short) * 2 + sizeof(float) * 3;
            }
            public STELandingGearSetup data { get; set; } = new STELandingGearSetup();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventDropBomb : SEvent
        {
            public SEventDropBomb() : base(EventID.DROP_BOMB)
            {
                size = sizeof(float) * 4 + sizeof(ushort);
            }
            public STEDropData data { get; set; } = new STEDropData();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventRocketLaunch : SEvent
        {
            public SEventRocketLaunch() : base(EventID.ROCKET_LAUNCH)
            {
                size = sizeof(float) * 4 + sizeof(ushort);
            }
            public STEDropData data { get; set; } = new STEDropData();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventHit : SEvent
        {
            public SEventHit() : base(EventID.HIT)
            {
                size = sizeof(float) * 6;
            }
            public STEHit data { get; set; } = new STEHit();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventDamage : SEvent
        {
            public SEventDamage() : base(EventID.DAMAGE)
            {
                size = sizeof(float) * 6;
            }
            public STEHit data { get; set; } = new STEHit();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventExplosion : SEvent
        {
            public SEventExplosion() : base(EventID.EXPLOSION)
            {
                size = sizeof(float) * 4;
            }
            public STEExplosion data { get; set; } = new STEExplosion();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventGunFire : SEvent
        {
            public SEventGunFire() : base(EventID.GUN_FIRE)
            {
                size = 1;
            }
            public byte data { get; set; } = 0;

            public override byte[] Serialize()
            {
                return new byte[] { (byte)id, size, data };
            }

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {(int)data}}}";
            }
        }

        public class SEventServerAddress : SEvent
        {
            public SEventServerAddress() : base(EventID.SRV_ADDR) { }
            public string data { get; set; } = "";

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {data}}}";
            }
        }

        public class SEventServerTitle : SEvent
        {
            public SEventServerTitle() : base(EventID.SRV_TITLE) { }
            public string data { get; set; } = "";

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {data}}}";
            }
        }

        public class SEventSRSAddress : SEvent
        {
            public SEventSRSAddress() : base(EventID.SRS_ADDR) { }
            public string data { get; set; } = "";

            public override byte[] Serialize()
            {
                return Encoding.ASCII.GetBytes(data);
            }
            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {data}}}";
            }
        }

        public class SEventClientData : SEvent
        {
            public SEventClientData() : base(EventID.CLIENT_DAT) { }
            public STClientData data { get; set; } = new STClientData();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
            }
        }

        public class SEventControlledData : SEvent
        {
            public SEventControlledData() : base(EventID.CTRL_DAT) { }
            public STControlledData data { get; set; } = new STControlledData();

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

            public override string ToString()
            {
                return $"{{id: {id.ToString()}, size: {(int)size}, data: {{{data.ToString()}}}}}";
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
            public List<SIndicator> indicators { get; set; } = new List<SIndicator>();
            public byte numOfEvents { get; set; } = 0;
            public List<SEvent> events { get; set; } = new List<SEvent>();

            public override string ToString()
            {
                if (size == 0)
                {
                    CalcSize();
                }
                string ser = $"{{id: {((MessageType)id).ToString()}, size: {size}, tick: {tick}, numOfIndicators: {(int)numOfIndicators}, indicators: [";
                foreach (SIndicator indicator in indicators)
                {
                    ser += $"{{id: {indicator.id}, numOfValues: {indicator.numOfValues}, values: [";
                    foreach (float value in indicator.values)
                    {
                        ser += $"{value}, ";
                    }                    
                }
                ser = ser.Remove(ser.Length - 2);
                ser += $"], numOfEvents: {numOfEvents}, events: [";
                foreach (SEvent ev in events)
                {
                    ser += $"{ev.ToString()}, ";
                }
                ser = ser.Remove(ser.Length - 2);
                ser += "]}";

                return ser;
            }

            public byte[] Serialize(ref string? debug)
            {
                if (size == 0)
                {
                    CalcSize();
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(id);
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

            private void CalcSize()
            {
                size = (ushort)(sizeof(byte) * 2 + (indicators.Count * sizeof(float)));
                foreach (SEvent ev in events)
                {
                    size += (ushort)ev.size;
                }
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

            public byte[] Serialize(ref string? debug)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(id);
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

            public override string ToString()
            {
                return $"{{id: {((MessageType)id).ToString()}, tick: {tick}, yaw: {yaw}, pitch: {pitch}, roll: {roll}, spin: [{spin[0]}, {spin[1]}, {spin[2]}], acc: [{acc[0]}, {acc[1]}, {acc[2]}]}}";
            }
        }
    } // namespace IL2Protocol
} // namespace IL2WinWing
