using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TrapTool
{
    public enum ShapeType : byte
    {
        Box = 19,
        Path = 27
    }

    public class TrapEntry : IXmlSerializable
    {
        public ShapeType Type { get; set; }
        public Tag Tags { get; set; }
        public FoxHash Name { get; set; }
        public List<ITrapShape> Shapes = new List<ITrapShape>();

        public void Read(BinaryReader reader, HashManager hashManager)
        {
            uint shapeDefBitfield = reader.ReadUInt32();
            Type = (ShapeType)(byte)(shapeDefBitfield & 0xFF);
            byte shapeDefUnknown0 = (byte)((byte)(shapeDefBitfield >> 8) & 0xFF); 
            if (shapeDefUnknown0 != 0) 
            { 
                throw new FormatException($"@{reader.BaseStream.Position} shapeDefUnknown0 is not 0!!! Is {shapeDefUnknown0}!!!"); 
            };
            byte shapeDefUnknown1 = (byte)((byte)(shapeDefBitfield >> 16) & 0xFF); 
            if (shapeDefUnknown1 != 128) 
            { 
                throw new FormatException($"@{reader.BaseStream.Position} shapeDefUnknown1 is not 0!!! Is {shapeDefUnknown1}!!!"); 
            };
            byte shapeCount = (byte)((byte)(shapeDefBitfield >> 24) & 0xFF);
            reader.ReadZeroes(12);
            Console.WriteLine($"@{reader.BaseStream.Position} Type: {Type}, shape#: {shapeCount}");

            Tags = (Tag)reader.ReadUInt64();
            Name = new FoxHash(FoxHash.Type.StrCode32);
            Name.Read(reader, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified);
            reader.ReadZeroes(4);
            Console.WriteLine($"@{reader.BaseStream.Position} Tags: {Tags}, Name: {Name.HashValue}");

            for (int i = 0; i < shapeCount; i++)
            {
                ITrapShape shape;
                switch (Type)
                {
                    case ShapeType.Box:
                        shape = new BoxShape();
                        break;
                    case ShapeType.Path:
                        shape = new GeoxTrapAreaPath();
                        break;
                    default:
                        throw new NotImplementedException($"@{reader.BaseStream.Position} Unknown ShapeType!!!");
                }

                shape.Read(reader);
                Shapes.Add(shape);
            }
            if (Type==ShapeType.Box)
            {
                reader.ReadZeroes(16);
            }

        }
        public void Write(BinaryWriter writer)
        {
            uint shapeDefBitfield = 0;
            shapeDefBitfield += (byte)Type;
            shapeDefBitfield += 0 << 8;
            shapeDefBitfield += 128 << 16;
            shapeDefBitfield += (uint)((byte)Shapes.Count << 24);
            writer.Write(shapeDefBitfield);
            writer.WriteZeroes(12);

            writer.Write((ulong)Tags);
            Name.Write(writer);
            writer.WriteZeroes(4);

            foreach (ITrapShape shape in Shapes)
            {
                shape.Write(writer);
            }

            if (Type == ShapeType.Box)
                writer.WriteZeroes(16);
        }

        public void ReadXml(XmlReader reader)
        {
            Name = new FoxHash(FoxHash.Type.StrCode32);
            Name.ReadXml(reader, "name");

            Type = (ShapeType)ShapeType.Parse(typeof(ShapeType), reader["type"]);
            reader.ReadStartElement("entry");

            Tags = 0;
            reader.ReadStartElement("tags");
            var loop = true;
            while (loop)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        var tag = reader["name"];
                        reader.ReadStartElement("tag");

                        Tags |= Tags.GetFromDescription(tag);
                        continue;
                    case XmlNodeType.EndElement:
                        loop = false;
                        break;
                }
            }
            reader.ReadEndElement();

            while (2 > 1)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        ITrapShape newShape = CreateShape();
                        newShape.ReadXml(reader);
                        reader.ReadEndElement();
                        Shapes.Add(newShape);
                        continue;
                    case XmlNodeType.EndElement:
                        return;
                }
            }
        }
        ITrapShape CreateShape()
        {
            return Type switch
            {
                ShapeType.Box => new BoxShape(),
                ShapeType.Path => new GeoxTrapAreaPath(),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public void WriteXml(XmlWriter writer)
        {
            Name.WriteXml(writer, "name");
            Console.WriteLine($"Name: {Name.HashValue}");
            writer.WriteAttributeString("type", Type.ToString());
            Console.WriteLine($"Type: {Type}");

            writer.WriteStartElement("tags");
            for (int bitIndex = 0; bitIndex < 64; bitIndex++)
            {
                ulong mask = (ulong)1 << bitIndex;
                Tag tag = (Tag)((ulong)Tags & mask);

                if (tag == 0)
                    continue;

                writer.WriteStartElement("tag");
                writer.WriteAttributeString("name", tag.GetDescription());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            foreach (ITrapShape shape in Shapes)
            {
                writer.WriteStartElement("shape");
                shape.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }
    }
}
