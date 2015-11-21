using System;
using Remact.Net.Bms1Serializer;

// see message documentation in https://github.com/steforster/bms1-binary-message-stream-format/blob/master/Example%20Interface%20Specification.md
namespace Remact.Net.Bms1UnitTest
{
    /// <summary>
    /// Empty message.
    /// </summary>
    public class IdleMessage : IBms1Dto
    {
        public UInt16 Bms1BlockTypeId { get { return 1; } }

        public static IdleMessage ReadFromBms1Stream(IBms1Reader reader)
        {
            return new IdleMessage();
        }

        public void WriteToBms1Stream(IBms1Writer writer)
        {
        }
    }


    /// <summary>
    /// Message containing a inherited class (BMS1-block)
    /// </summary>
    public class IdentificationMessage : IBms1Dto
    {
        public UInt16 Bms1BlockTypeId { get { return 2; } }

        public string InterfaceName;
        public Int16 InterfaceVersion;
        public string ApplicationName;
        public VersionBase ApplicationVersion;
        public string ApplicationInstance;

        public static IdentificationMessage ReadFromBms1Stream(IBms1Reader reader)
        {
            return new IdentificationMessage
            {
                InterfaceName = reader.ReadString(),
                InterfaceVersion = reader.ReadInt16(),
                ApplicationName = reader.ReadString(),
                ApplicationVersion = reader.ReadBlock(VersionBase.ReadFromBms1Stream),
                ApplicationInstance = reader.ReadString(),
            };
        }

        public void WriteToBms1Stream(IBms1Writer writer)
        {
            writer.WriteString(InterfaceName);
            writer.WriteInt16(InterfaceVersion);
            writer.WriteString(ApplicationName);

            // TODO writer.WriteBlock(ApplicationVersion);

            writer.WriteString(ApplicationInstance);
        }
    }

    /// <summary>
    /// The base class of a message member (BMS1 block).
    /// </summary>
    public abstract class VersionBase
    {
        public virtual UInt16 Bms1BlockTypeId { get { return 100; } }

        public static VersionBase ReadFromBms1Stream(IBms1Reader reader)
        {
            var type = reader.Internal.BlockTypeId;
            if (type == 101)
            {
                return VersionDotNet.ReadFromBms1Stream(reader);
            }
            else if (type == 102)
            {
                return VersionPLC.ReadFromBms1Stream(reader);
            }
            else
            {   // TODO: test it, it reads another block-frame ?
                return reader.ReadBlock<VersionBase>(null); // skip when null or unknown block type
            }
        }

        public abstract void WriteToBms1Stream(IBms1Writer writer);
    }

    public class VersionDotNet : VersionBase
    {
        public override UInt16 Bms1BlockTypeId { get { return 101; } }

        public Version Version;

        new public static VersionDotNet ReadFromBms1Stream(IBms1Reader reader)
        {
            return new VersionDotNet
            {
                Version = default(Version).ReadFromBms1Stream(reader)
            };
        }

        public override void WriteToBms1Stream(IBms1Writer writer)
        {
            Version.WriteToBms1Stream(writer);
        }
    }

    /// <summary>
    /// A derieved BMS1 block.
    /// </summary>
    public class VersionPLC : VersionBase
    {
        public override UInt16 Bms1BlockTypeId { get { return 102; } }

        public string Version;
        public CpuType CpuType; // Message V.2

        new public static VersionPLC ReadFromBms1Stream(IBms1Reader reader)
        {
            return new VersionPLC
            {
                Version = reader.ReadString(),
            };
        }

        public override void WriteToBms1Stream(IBms1Writer writer)
        {
            writer.WriteString(Version);
            // TODO writer.WriteEnum(CpuType);
        }
    }

    public enum CpuType
    {
        Unknown = 0,
        ArmCortexA5 = 2,
        ArmCortexA9 = 3
    }

    public static class MessageExtensions
    {
        /// <summary>
        /// Here we demonstrate how to stream a member class of a library.
        /// The extension method reads System.Version from a BMS1 stream.
        /// </summary>
        public static Version ReadFromBms1Stream(this Version version, IBms1Reader reader)
        {
            return new Version(reader.ReadInt32(),
                               reader.ReadInt32(),
                               reader.ReadInt32(),
                               reader.ReadInt32());
        }

        /// <summary>
        /// Here we demonstrate how to stream a member class of a library.
        /// The extension method writes System.Version to a BMS1 stream.
        /// </summary>
        public static void WriteToBms1Stream(this Version version, IBms1Writer writer)
        {
            writer.WriteInt32(version.Major);
            writer.WriteInt32(version.Minor);
            writer.WriteInt32(version.Revision);
            writer.WriteInt32(version.Build);
        }
    }
}

