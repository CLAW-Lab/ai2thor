using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
using PhysicsRemoteFPSAgentController = UnityStandardAssets.Characters.FirstPerson.PhysicsRemoteFPSAgentController;

namespace MessagePack.Formatters {
    using System;
    using System.Buffers;
    using MessagePack;

    public sealed class ActionReturnFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Object> {


        private readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        private readonly byte[][] ____stringByteKeys;

        public ActionReturnFormatter() {
        }

        public void Serialize(ref MessagePackWriter writer, global::System.Object value, global::MessagePack.MessagePackSerializerOptions options) {
            #if !ENABLE_IL2CPP
                IMessagePackFormatter<global::System.Object> dynamicFormatter = (IMessagePackFormatter<global::System.Object>)DynamicObjectTypeFallbackFormatter.Instance;
                dynamicFormatter.Serialize(ref writer, value, options);
                return;
            #endif
        
            if (value == null) {
                writer.WriteNil();
                return;
            }
            Type type = value.GetType();
            TypeInfo ti = type.GetTypeInfo();
            if (type == typeof(Vector3[])) {
                var values = (Vector3[])value;
                writer.WriteArrayHeader(values.Length);
                var formatter = options.Resolver.GetFormatterWithVerify<Vector3>();

                foreach (var f in values) {
                    formatter.Serialize(ref writer, f, options);
                }

            } else if (value is System.Collections.IDictionary) {
                var d = value as System.Collections.IDictionary;
                writer.WriteMapHeader(d.Count);
                foreach (System.Collections.DictionaryEntry item in d) {
                    this.Serialize(ref writer, item.Key, options);
                    this.Serialize(ref writer, item.Value, options);
                }

                return;
            } else if (value is System.Collections.ICollection) {
                var c = value as System.Collections.ICollection;
                writer.WriteArrayHeader(c.Count);
                foreach (var item in c) {
                    this.Serialize(ref writer, item, options);
                }

                return;
            } else {
                // all custom types that could appear in actionReturn
                // must appear here
                if (type == typeof(Vector3)) {
                    options.Resolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, (Vector3)value, options);
                } else if (type == typeof(InitializeReturn)) {
                    options.Resolver.GetFormatterWithVerify<InitializeReturn>().Serialize(ref writer, (InitializeReturn)value, options);
                } else if (type == typeof(PhysicsRemoteFPSAgentController.WhatDidITouch)) {
                    options.Resolver.GetFormatterWithVerify<PhysicsRemoteFPSAgentController.WhatDidITouch>().Serialize(ref writer, (PhysicsRemoteFPSAgentController.WhatDidITouch)value, options);
                } else if (type == typeof(UnityEngine.AI.NavMeshPath)) {
                    options.Resolver.GetFormatterWithVerify<UnityEngine.AI.NavMeshPath>().Serialize(ref writer, (UnityEngine.AI.NavMeshPath)value, options);
                } else if (PrimitiveObjectFormatter.IsSupportedType(type, ti, value)) {
                    PrimitiveObjectFormatter.Instance.Serialize(ref writer, value, options);
                } else {
                    throw new MessagePackSerializationException("Not supported type: " + type.Name);
                }
            }
        }
        public global::System.Object Deserialize(ref MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options) {
            return null;
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
