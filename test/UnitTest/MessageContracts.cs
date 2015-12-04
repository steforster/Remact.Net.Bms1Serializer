using System;
using Remact.Net.Bms1Serializer;

// see message documentation in https://github.com/steforster/bms1-binary-message-stream-format/blob/master/Example%20Interface%20Specification.md
namespace Remact.Net.Bms1UnitTest
{
    /// <summary>
    /// Empty message.
    /// </summary>
    public class IdleMessage
    {
        public const int Bms1BlockTypeId = 1;

        public static IdleMessage ReadFromBms1Stream(IBms1Reader reader)
        {
            // ReadBlock calls us back, in case the serialized value is not null.
            return reader.ReadBlock<IdleMessage>((dto) => new IdleMessage());
        }

        public void WriteToBms1Stream(IBms1Writer writer)
        {
            writer.WriteBlock(Bms1BlockTypeId, () => { });
        }
    }


    /// <summary>
    /// Message containing a inherited class.
    /// </summary>
    public class IdentificationMessage
    {
        public const int Bms1BlockTypeId = 2;

        public string InterfaceName;
        public Int16 InterfaceVersion;
        public string ApplicationName;
        public VersionBase ApplicationVersion;
        public string ApplicationInstance;

        
        public static IdentificationMessage ReadFromBms1Stream(IBms1Reader reader)
        {
            return reader.ReadBlock<IdentificationMessage>(
                (dto) =>
                {
                    dto.InterfaceName = reader.ReadString();
                    dto.InterfaceVersion = reader.ReadInt16();
                    dto.ApplicationName = reader.ReadString();

                    dto.ApplicationVersion = VersionBase.ReadFromBms1Stream(reader);

                    dto.ApplicationInstance = reader.ReadString();
                    return dto;
                });
        }

        public void WriteToBms1Stream(IBms1Writer writer)
        {
            writer.WriteBlock(Bms1BlockTypeId, () =>
                {
                    writer.WriteString(InterfaceName);
                    writer.WriteInt16(InterfaceVersion);
                    writer.WriteString(ApplicationName);

                    ApplicationVersion.WriteToBms1Stream(writer);

                    writer.WriteString(ApplicationInstance);
                });
        }
    }

    /// <summary>
    /// The base class can read or write its derieved from/to BMS1 stream.
    /// </summary>
    public abstract class VersionBase
    {
        public abstract void WriteToBms1Stream(IBms1Writer writer);

        public static VersionBase ReadFromBms1Stream(IBms1Reader reader)
        {
            var type = reader.Internal.BlockTypeId;
            if (type == VersionDotNet.Bms1BlockTypeId)
            {
                return VersionDotNet.CreateFromBms1Stream(reader);
            }
            else if (type == VersionPLC.Bms1BlockTypeId)
            {
                return VersionPLC.CreateFromBms1Stream(reader);
            }
            else
            {   // TODO: test it, it reads another block-frame ?
                return reader.ReadBlock<VersionPLC>(null); // skip when null or unknown block type
            }
        }
    }


    /// <summary>
    /// A streamable, derieved class.
    /// </summary>
    public class VersionPLC : VersionBase
    {
        public const int Bms1BlockTypeId = 102;

        public string Version;
        public CpuType CpuType;
        public string AdditionaInfo = "None"; // not transferred before V.2

        internal static VersionPLC CreateFromBms1Stream(IBms1Reader reader)
        {
            return reader.ReadBlock<VersionPLC>(
                (dto) =>
                {
                    dto.Version = reader.ReadString();
                    dto.CpuType = reader.ReadEnum<CpuType>();
                    if (!reader.Internal.EndOfBlock)
                    {
                        // starting with V.2, more data is transferred. In V.1, the AdditionaInfo is set to "None"
                        dto.AdditionaInfo = reader.ReadString();
                    }
                    return dto;
                });
        }

        public override void WriteToBms1Stream(IBms1Writer writer)
        {
            writer.WriteBlock(Bms1BlockTypeId, () =>
            {
                writer.WriteString(Version);
                writer.WriteEnum(CpuType);
                // Intentionally, we do not write AdditionalInfo. This simulates an older sender and a newer receiver;
            });
        }
    }

    /// <summary>
    /// A streamable enumeration.
    /// </summary>
    public enum CpuType
    {
        Unknown = 0,
        ArmCortexA5 = 2,
        ArmCortexA9 = 3
    }

    /// <summary>
    /// Another streamable, derieved class. This class contains a .NET system library type and streams it using extension methods.
    /// </summary>
    public class VersionDotNet : VersionBase
    {
        public const int Bms1BlockTypeId = 101;
        public Version Version;

        internal static VersionDotNet CreateFromBms1Stream(IBms1Reader reader)
        {
            return reader.ReadBlock<VersionDotNet>(
                (dto) =>
                {
                    dto.Version = MessageExtensions.ReadFromBms1Stream(null, reader);
                    return dto;
                });
        }

        public override void WriteToBms1Stream(IBms1Writer writer)
        {
            writer.WriteBlock(Bms1BlockTypeId, () =>
                {
                    Version.WriteToBms1Stream(writer);
                });
        }
    }

    
    public static class MessageExtensions
    {
        /// <summary>
        /// Here we demonstrate how to stream a member class of a closed library.
        /// The extension method reads System.Version from a BMS1 stream.
        /// </summary>
        public static Version ReadFromBms1Stream(this Version version, IBms1Reader reader)
        {
            return reader.ReadBlock<Version>(
                (dto) =>
                {
                    return new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                });
        }

        /// <summary>
        /// Here we demonstrate how to stream a member class of a closed library.
        /// The extension method writes System.Version to a BMS1 stream.
        /// </summary>
        public static void WriteToBms1Stream(this Version version, IBms1Writer writer)
        {
            writer.WriteBlock(() =>
                {
                    writer.WriteInt32(version.Major);
                    writer.WriteInt32(version.Minor);
                    writer.WriteInt32(version.Build);
                    writer.WriteInt32(version.Revision);
                });
        }
    }
}

