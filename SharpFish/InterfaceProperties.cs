using System;
using System.IO.Ports;

namespace SharpFish
{
    public enum InputDigital
    {
        E1,
        E2,
        E3,
        E4,
        E5,
        E6,
        E7,
        E8
    }

    public enum InputAnalog
    {
        EX,
        EY
    }

    public enum OutputMotor
    {
        M1,
        M2,
        M3,
        M4
    }

    public enum MotorState
    {
        Left = 1,
        Off = 0,
        Right = -1,
    }

    /// <summary>
    /// Contains the properties of the interface
    /// </summary>
    internal static class InterfaceProperties
    {
        /// <summary>
        /// The Baudrate of the interface COMPort
        /// </summary>
        public static readonly int ComBaudRate = 9600;
        /// <summary>
        /// The number of Databits COMPort
        /// </summary>
        public static readonly int ComDataBits = 8;
        /// <summary>
        /// What parity bit is used on COMPort
        /// </summary>
        public static readonly Parity ComParityBit = Parity.None;
        /// <summary>
        /// The number of stopbits COMPort
        /// </summary>
        public static readonly StopBits ComStopBits = System.IO.Ports.StopBits.One;
        /// <summary>
        /// Total number of bits per packet: DataBits + ParityBit + StopBits
        /// </summary>
        public static readonly int ComBitsTotal = 9;


        /// <summary>
        /// The command for the interface to read just the digital inputs
        /// </summary>
        public static readonly byte CmdReadE = 0xC1;
        /// <summary>
        /// The command for the interface to read digital inputs and EX analog input
        /// </summary>
        public static readonly byte CmdReadEX = 0xC5;
        /// <summary>
        /// The command for the interface to read digital inputs and EY analog input
        /// </summary>
        public static readonly byte CmdReadEY = 0xC9;
    }
}
