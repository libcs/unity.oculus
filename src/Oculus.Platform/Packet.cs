namespace Oculus.Platform
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class Packet : IDisposable
    {
        readonly IntPtr packetHandle;

        public Packet(IntPtr packetHandle)
        {
            this.packetHandle = packetHandle;
            Size = (ulong)CAPI.ovr_Packet_GetSize(packetHandle);
        }
        ~Packet() { Dispose(); }
        public void Dispose()
        {
            CAPI.ovr_Packet_Free(packetHandle);
            GC.SuppressFinalize(this);
        }

        /**
         * Copies all the bytes in the payload into byte[] destination.  ex:
         *   Package package ...
         *   byte[] destination = new byte[package.Size];
         *   package.ReadBytes(destination);
         */
        public ulong ReadBytes(byte[] destination)
        {
            if ((ulong)destination.LongLength < Size)
                throw new ArgumentException(string.Format("Destination array was not big enough to hold {0} bytes", Size));
            Marshal.Copy(CAPI.ovr_Packet_GetBytes(packetHandle), destination, 0, (int)Size);
            return Size;
        }

        public ulong SenderID => CAPI.ovr_Packet_GetSenderID(packetHandle);
        public ulong Size { get; }
        public SendPolicy Policy => CAPI.ovr_Packet_GetSendPolicy(packetHandle);
    }
}
