using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using IL2WinWing.IL2Protocol;

namespace IL2WinWing
{
    internal class DummyData
    {
        private static IL2Protocol.Telemetry telemetry = new IL2Protocol.Telemetry();
        //private IL2Protocol.Motion motion;

        public static byte[] GetTelemetry(ref string? debug)
        {
            if (telemetry.numOfIndicators == 0)
            {
                telemetry.numOfIndicators = 5;
                telemetry.indicators.Add(new IL2Protocol.SIndicator(IL2Protocol.IndicatorID.ENG_RPM));
                telemetry.indicators.Add(new IL2Protocol.SIndicator(IL2Protocol.IndicatorID.ENG_MP));
                telemetry.indicators.Add(new IL2Protocol.SIndicator(IL2Protocol.IndicatorID.EAS));
                telemetry.indicators.Add(new IL2Protocol.SIndicator(IL2Protocol.IndicatorID.AOA));
                telemetry.indicators.Add(new IL2Protocol.SIndicator(IL2Protocol.IndicatorID.AGL));

                for (int ix = 0; ix < telemetry.numOfIndicators; ix++)
                {
                    telemetry.indicators[ix].numOfValues = 1;
                    telemetry.indicators[ix].values.Add(0.0F);
                }

                telemetry.numOfEvents = 4;
                telemetry.events.Add(new IL2Protocol.SEventSetFocus("=NOSIG=ZenIT_SWE"));

                var setupEngine = new IL2Protocol.SEventSetupEngine();
                setupEngine.data.afPos[0] = 10.0F;
                setupEngine.data.afPos[1] = 15.0F;
                setupEngine.data.afPos[2] = -15.0F;
                setupEngine.data.fMaxRPM = 3000.0F;
                telemetry.events.Add(setupEngine);

                var setupGuns = new IL2Protocol.SEventSetupGun();
                setupGuns.data.afPos[0] = 30.0F;
                setupGuns.data.afPos[1] = 35.0F;
                setupGuns.data.afPos[2] = -35.0F;
                setupGuns.data.fProjectileMass = 0.1F;
                setupGuns.data.fShootVelocity = 1000.0F;
                telemetry.events.Add(setupGuns);

                var ldgGear = new IL2Protocol.SEventSetupLandingGear();
                ldgGear.data.afPos[0] = 50.0F;
                ldgGear.data.afPos[1] = 55.0F;
                ldgGear.data.afPos[2] = -55.0F;
                telemetry.events.Add(ldgGear);
            }

            return telemetry.Serialize(ref debug);
        }
    }
}
