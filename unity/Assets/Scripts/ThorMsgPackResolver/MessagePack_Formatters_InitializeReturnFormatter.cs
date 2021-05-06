using UnityEngine;
// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters
{
    using System;
    using System.Buffers;
    using MessagePack;

    public sealed class InitializeReturnFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::InitializeReturn>
    {


        private readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        private readonly byte[][] ____stringByteKeys;

        public InitializeReturnFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "cameraNearPlane", 0 },
                { "cameraFarPlane", 1 },
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.Internal.CodeGenHelpers.GetEncodedStringBytes("cameraNearPlane"),
                global::MessagePack.Internal.CodeGenHelpers.GetEncodedStringBytes("cameraFarPlane"),
            };
        }

        public void Serialize(ref MessagePackWriter writer, global::InitializeReturn value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteMapHeader(2);
            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.Write(value.cameraNearPlane);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.Write(value.cameraFarPlane);
        }

        public global::InitializeReturn Deserialize(ref MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var __cameraNearPlane__ = default(float);
            var __cameraFarPlane__ = default(float);

            for (int i = 0; i < length; i++)
            {
                ReadOnlySpan<byte> stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                int key;
                if (!this.____keyMapping.TryGetValue(stringKey, out key))
                {
                    reader.Skip();
                    continue;
                }

                switch (key)
                {
                    case 0:
                        __cameraNearPlane__ = reader.ReadSingle();
                        break;
                    case 1:
                        __cameraFarPlane__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::InitializeReturn();
            ____result.cameraNearPlane = __cameraNearPlane__;
            ____result.cameraFarPlane = __cameraFarPlane__;
            reader.Depth--;
            return ____result;
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
