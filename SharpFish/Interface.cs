using System;
using System.Threading;

namespace SharpFish
{
    public class Interface
    {
        private InterfaceCom com;

        private AutoResetEvent waitHandle;

        private SealedConcurrentDictionary<InputDigital, bool> inDigital;
        private SealedConcurrentDictionary<InputAnalog, int> inAnalog;
        private SealedConcurrentDictionary<OutputMotor, MotorState> outMotor;

        public bool E1 {

            get { return inDigital[InputDigital.E1]; }
        }

        public bool E2 {

            get { return inDigital[InputDigital.E2]; }
        }

        public bool E3 {

            get { return inDigital[InputDigital.E3]; }
        }

        public bool E4 {

            get { return inDigital[InputDigital.E4]; }
        }

        public bool E5 {

            get { return inDigital[InputDigital.E5]; }
        }

        public bool E6 {

            get { return inDigital[InputDigital.E6]; }
        }

        public bool E7 {

            get { return inDigital[InputDigital.E7]; }
        }

        public bool E8 {

            get { return inDigital[InputDigital.E8]; }
        }

        public int EX {

            get { return inAnalog[InputAnalog.EX]; }
        }

        public int EY {

            get { return inAnalog[InputAnalog.EY]; }
        }

        public MotorState M1
        {

            get { return outMotor[OutputMotor.M1]; }
            set { outMotor[OutputMotor.M1] = value; }
        }

        public MotorState M2
        {

            get { return outMotor[OutputMotor.M2]; }
            set { outMotor[OutputMotor.M2] = value; }
        }

        public MotorState M3
        {

            get { return outMotor[OutputMotor.M3]; }
            set { outMotor[OutputMotor.M3] = value; }
        }

        public MotorState M4
        {

            get { return outMotor[OutputMotor.M4]; }
            set { outMotor[OutputMotor.M4] = value; }
        }

        /// <summary>
        /// Creates a new Interface to communicate with
        /// </summary>
        /// <param name="ComPortName">The name of the COM port</param>
        public Interface(string ComPortName)
        {
            waitHandle = new AutoResetEvent(false);
            inDigital = new SealedConcurrentDictionary<InputDigital, bool>((InputDigital[])Enum.GetValues(typeof(InputDigital)));
            inAnalog = new SealedConcurrentDictionary<InputAnalog, int>((InputAnalog[])Enum.GetValues(typeof(InputAnalog)));
            outMotor = new SealedConcurrentDictionary<OutputMotor, MotorState>((OutputMotor[])Enum.GetValues(typeof(OutputMotor)));



            com = new InterfaceCom(ComPortName, waitHandle, inDigital, inAnalog, outMotor);
        }

        /// <summary>
        /// Establishes connection with the interface and start communicating 
        /// </summary>
        public void Connect()
        {
            com.Start();
        }

        /// <summary>
        /// Disconnect interface. Blocks until conntion fully disconnected
        /// </summary>
        public void Disconnect()
        {
            com.Stop();
        }

        /// <summary>
        /// Waits for a DigitalInput to occur in a give state
        /// </summary>
        /// <param name="e">The input port to wait for</param>
        /// <param name="level">The signal state to wait for</param>
        public void WaitFor(InputDigital e, bool level)
        {
            while (inDigital[e] != level)
            {
                waitHandle.WaitOne();
            }
        }
    }
}
