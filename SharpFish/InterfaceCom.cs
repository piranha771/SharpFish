using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace SharpFish
{
    /// <summary>
    /// Interface communicator thread
    /// </summary>
    internal class InterfaceCom
    {

        private static readonly int ReadWriteTimeout = 1000;

        private SerialPort port;
        private int sendDelay;

        private SealedConcurrentDictionary<InputDigital, bool> inDigital;
        private SealedConcurrentDictionary<InputAnalog, int> inAnalog;
        private SealedConcurrentDictionary<OutputMotor, MotorState> outMotor;
        
        private Thread comThread;
        private object stopLock = new object();
        private readonly AutoResetEvent waitHandle;
        private volatile bool stop;

        /// <summary>
        /// Returns true while the communication thread is running
        /// </summary>
        public bool IsAlive { get { return comThread != null && comThread.IsAlive; } }

        /// <summary>
        /// Creates a new Interface COM communication 
        /// </summary>
        /// <param name="comName">Name of the COM port</param>
        /// <param name="inDigital">The dictionary that holds the digital state</param>
        /// <param name="inAnalog">The dictionary that holds the analog state</param>
        /// <param name="outMotor">The dictionary that holds the motor state</param>
        public InterfaceCom(string comName, AutoResetEvent waitHandle, SealedConcurrentDictionary<InputDigital, bool> inDigital, SealedConcurrentDictionary<InputAnalog, int> inAnalog, SealedConcurrentDictionary<OutputMotor, MotorState> outMotor)
        {
            this.inDigital = inDigital;
            this.inAnalog = inAnalog;
            this.outMotor = outMotor;
            this.waitHandle = waitHandle;

            sendDelay = 50;//(int)Math.Ceiling((1000.0 / (InterfaceProperties.ComBaudRate / InterfaceProperties.ComBitsTotal * 10f))); // 10 = total packets for one readwrite (2 * 2 send + 2 * 3 receive)

            try 
            { 
                port = new SerialPort(comName, InterfaceProperties.ComBaudRate, InterfaceProperties.ComParityBit, InterfaceProperties.ComDataBits, InterfaceProperties.ComStopBits);
                port.WriteTimeout = ReadWriteTimeout;
                port.ReadTimeout = ReadWriteTimeout;
            }
            catch (IOException e)
            {
                throw new InvalidDataException("The COM name is invalid, or COM port is blocked. \n\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Starts the communication thread
        /// </summary>
        public void Start()
        {
            if (comThread != null && comThread.IsAlive) throw new InvalidOperationException("Communication thread, is already running! First stop the old to create a new one.");
            comThread = new Thread(this.Run);
            comThread.Name = "Inteface Communication Thread";
            comThread.Start();
        }

        /// <summary>
        /// Stops the coimmunicator thread. Blocks until fully stopped
        /// </summary>
        public void Stop()
        {
            if (comThread != null && comThread.IsAlive)
            {
                stop = true;
                lock (stopLock) { }
            }
        }

        private void Run()
        {
            lock (stopLock)
            {
                try
                {
                    port.Open();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("The COM name is invalid, or COM port is blocked. \n\n" + e.StackTrace);
                }
                try
                {
                    ReadWrite();
                }
                finally
                {
                    port.Close();
                }
            }
        }

        private void ReadWrite()
        {

            byte[] sendBuffer = new byte[2];
            byte[] receiveBuffer = new byte[3];
            bool digitalChanges = false;
            while (!stop)
            {
                // -- EX
                // Send current motor state to interface and read all digitals inputs and EX analog input
                sendBuffer[0] = InterfaceProperties.CmdReadEX;
                sendBuffer[1] = CreateMotorByte();
                port.Write(sendBuffer, 0, sendBuffer.Length);

                // Read the response from the interface and set the states
                port.Read(receiveBuffer, 0, receiveBuffer.Length);
                digitalChanges = WriteDigitalInputs(receiveBuffer[0]);
                inAnalog[InputAnalog.EX] = TwoBytesToInt(receiveBuffer[1], receiveBuffer[2]);

                // -- EY
                // Send current motor state to interface and read all digitals inputs and EY analog input
                sendBuffer[0] = InterfaceProperties.CmdReadEY;
                sendBuffer[1] = CreateMotorByte();
                port.Write(sendBuffer, 0, sendBuffer.Length);

                // Read the response from the interface and set the states
                port.Read(receiveBuffer, 0, receiveBuffer.Length);
                digitalChanges |= WriteDigitalInputs(receiveBuffer[0]);
                inAnalog[InputAnalog.EY] = TwoBytesToInt(receiveBuffer[1], receiveBuffer[2]);

                if (digitalChanges) waitHandle.Set();
                Thread.Sleep(sendDelay);
            }
        }

        private byte CreateMotorByte()
        {
            byte ret = 0;
            ret |= (byte)(outMotor[OutputMotor.M1] == MotorState.Left ?  1 : 0); // left
            ret |= (byte)(2 * (outMotor[OutputMotor.M4] == MotorState.Right ? 1 : 0)); // right

            ret |= (byte)(4 * (outMotor[OutputMotor.M2] == MotorState.Left ? 1 : 0)); // left
            ret |= (byte)(8 * (outMotor[OutputMotor.M2] == MotorState.Right ? 1 : 0)); // right

            ret |= (byte)(16 * (outMotor[OutputMotor.M3] == MotorState.Left ? 1 : 0)); // left
            ret |= (byte)(32 * (outMotor[OutputMotor.M3] == MotorState.Right ? 1 : 0)); // right

            ret |= (byte)(64 * (outMotor[OutputMotor.M4] == MotorState.Left ? 1 : 0)); // left
            ret |= (byte)(128 * (outMotor[OutputMotor.M4] == MotorState.Right ? 1 : 0)); // right

            return ret;
        }

        /// <summary>
        /// Writes digital inputs to dictionary
        /// </summary>
        /// <param name="inputs">The digital inputs as bytes</param>
        /// <returns>True if there where changes</returns>
        private bool WriteDigitalInputs(byte inputs) {
            bool changes = false;
            bool old;

            old = inDigital[InputDigital.E1];
            inDigital[InputDigital.E1] = (inputs & 1)   == 1;
            changes |= inDigital[InputDigital.E1] == old;

            old = inDigital[InputDigital.E2];
            inDigital[InputDigital.E2] = (inputs & 2)   == 2;
            changes |= inDigital[InputDigital.E2] == old;

            old = inDigital[InputDigital.E3];
            inDigital[InputDigital.E3] = (inputs & 4)   == 4;
            changes |= inDigital[InputDigital.E3] == old;

            old = inDigital[InputDigital.E4];
            inDigital[InputDigital.E4] = (inputs & 8)   == 8;
            changes |= inDigital[InputDigital.E4] == old;

            old = inDigital[InputDigital.E5];
            inDigital[InputDigital.E5] = (inputs & 16)  == 16;
            changes |= inDigital[InputDigital.E5] == old;

            old = inDigital[InputDigital.E6];
            inDigital[InputDigital.E6] = (inputs & 32)  == 32;
            changes |= inDigital[InputDigital.E6] == old;

            old = inDigital[InputDigital.E7];
            inDigital[InputDigital.E7] = (inputs & 64)  == 64;
            changes |= inDigital[InputDigital.E7] == old;

            old = inDigital[InputDigital.E8];
            inDigital[InputDigital.E8] = (inputs & 128) == 128;
            changes |= inDigital[InputDigital.E8] == old;

            return changes;
        }

        private int TwoBytesToInt(byte first, byte second)
        {
            int ret = first << 8;
            return ret | second;
        }
   }
}
