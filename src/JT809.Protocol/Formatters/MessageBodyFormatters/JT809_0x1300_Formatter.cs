﻿using JT809.Protocol.Attributes;
using JT809.Protocol.Enums;
using JT809.Protocol.Exceptions;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace JT809.Protocol.Formatters.MessageBodyFormatters
{
    public class JT809_0x1300_Formatter : IJT809Formatter<JT809_0x1300>
    {
        public JT809_0x1300 Deserialize(ReadOnlySpan<byte> bytes, out int readSize)
        {
            int offset = 0;
            JT809_0x1300 jT809_0X1300 = new JT809_0x1300();
            jT809_0X1300.SubBusinessType = (JT809SubBusinessType)JT809BinaryExtensions.ReadUInt16Little(bytes, ref offset);
            jT809_0X1300.DataLength = JT809BinaryExtensions.ReadUInt32Little(bytes, ref offset);
            //JT809.Protocol.Enums.JT809BusinessType 映射对应消息特性
            JT809BodiesTypeAttribute jT809SubBodiesTypeAttribute = jT809_0X1300.SubBusinessType.GetAttribute<JT809BodiesTypeAttribute>();
            if (jT809SubBodiesTypeAttribute == null)
            {
                throw new JT809Exception(JT809ErrorCode.GetAttributeError, $"JT809BodiesTypeAttribute Not Found>{jT809_0X1300.SubBusinessType.ToString()}");
            }
            try
            {
                jT809_0X1300.SubBodies = JT809FormatterResolverExtensions.JT809DynamicDeserialize(JT809FormatterExtensions.GetFormatter(jT809SubBodiesTypeAttribute.JT809BodiesType), bytes.Slice(offset, (int)jT809_0X1300.DataLength), out readSize);
            }
            catch
            {
                throw new JT809Exception(JT809ErrorCode.SubBodiesParseError, $"SubBusinessType>{jT809_0X1300.SubBusinessType.ToString()}");
            }
            readSize = offset;
            return jT809_0X1300;
        }

        public int Serialize(ref byte[] bytes, int offset, JT809_0x1300 value)
        {
            offset += JT809BinaryExtensions.WriteUInt16Little(bytes, offset, (ushort)value.SubBusinessType);
            //offset += JT809BinaryExtensions.WriteUInt32Little(memoryOwner, offset, value.DataLength);
            //JT809.Protocol.Enums.JT809BusinessType 映射对应消息特性
            JT809BodiesTypeAttribute jT809SubBodiesTypeAttribute = value.SubBusinessType.GetAttribute<JT809BodiesTypeAttribute>();
            if (jT809SubBodiesTypeAttribute == null)
            {
                throw new JT809Exception(JT809ErrorCode.GetAttributeError, $"JT809BodiesTypeAttribute Not Found>{value.SubBusinessType.ToString()}");
            }
            try
            {
                // 先写入内容，然后在根据内容反写内容长度
                offset = offset + 4;
                int contentOffset = JT809FormatterResolverExtensions.JT809DynamicSerialize(JT809FormatterExtensions.GetFormatter(jT809SubBodiesTypeAttribute.JT809BodiesType),ref bytes, offset, value.SubBodies);
                JT809BinaryExtensions.WriteUInt32Little(bytes, offset - 4, (uint)(contentOffset - offset));
                offset = contentOffset;
            }
            catch
            {
                throw new JT809Exception(JT809ErrorCode.SubBodiesParseError, $"SubBusinessType>{value.SubBusinessType.ToString()}");
            }
            return offset;
        }
    }
}
