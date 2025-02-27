﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace TrapTool
{
    [Flags]
    public enum Tag : ulong
    {
        [Description("Intrude")]
        Intrude = 0x1,
        [Description("Tower")]
        Tower = 0x2,
        [Description("InRoom")]
        InRoom = 0x4,
        [Description("FallDeath")]
        FallDeath = 0x8,

        [Description("NearCamera1")]
        NearCamera1 = 0x10,
        [Description("NearCamera2")]
        NearCamera2 = 0x20,
        [Description("NearCamera3")]
        NearCamera3 = 0x40,
        [Description("NearCamera4")]
        NearCamera4 = 0x80,


        [Description("0x9978c8d36f7")]
        _9978c8d36f7 = 0x100,
        [Description("NoRainEffect")]
        NoRainEffect = 0x200,
        [Description("0x60e79a58dcc3")]
        _60e79a58dcc3 = 0x400,
        [Description("GimmickNoFulton")]
        GimmickNoFulton = 0x800,

        [Description("innerZone")]
        innerZone = 0x1000,
        [Description("outerZone")]
        outerZone = 0x2000,
        [Description("hotZone")]
        hotZone = 0x4000,
        [Description("0x439898dcbf83")]
        _439898dcbf83 = 0x8000,

        [Description("0xe780e431a068")]
        _e780e431a068 = 0x10000,
        [Description("0x53827eed3fbc")]
        _53827eed3fbc = 0x20000,
        [Description("0x7e1121c5cb93")]
        _7e1121c5cb93 = 0x40000,
        [Description("0xcadd57b76a83")]
        _cadd57b76a83 = 0x80000,

        [Description("0xe689072c4df8")]
        _e689072c4df8 = 0x100000,
        [Description("0x6d14396ebbe5")]
        _6d14396ebbe5 = 0x200000,
        [Description("0xd1ee7dc34fff")]
        _d1ee7dc34fff = 0x400000,
        [Description("0xb07e254afcae")]
        _b07e254afcae = 0x800000,

        [Description("Unnamed0")]
        Unnamed0 = 0x2000000,
        //[Description("Unnamed2")]
        //Unnamed0 = 0x2000000,
        //[Description("Unnamed3")]
        //Unnamed0 = 0x2000000,

        [Description("0xd6ee65d20b7a")]
        _d6ee65d20b7a = 0x10000000,
        [Description("0xf287ba9cb7e3")]
        _f287ba9cb7e3 = 0x20000000,
        [Description("NoFulton")]
        NoFulton = 0x40000000,
        [Description("Unnamed1")]
        Unnamed1 = 0x80000000,
        [Description("0x24330b0e33cb")]
        _24330b0e33cb = 0xffffffff80000000,
    };

    public static class TagExtensions
    {
        private static readonly Tag dummyTag = new Tag();

        public static string GetDescription(this Tag value)
        {
            FieldInfo fieldInfo = typeof(Tag).GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static Tag GetFromDescription(this Tag value, string description)
        {
            FieldInfo[] fieldInfos = typeof(Tag).GetFields();
            foreach (var fieldInfo in fieldInfos)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    if (attributes[0].Description == description)
                    {
                        return (Tag)fieldInfo.GetValue(dummyTag);
                    }
                }
                else
                {
                    continue;
                }
            }

            throw new ArgumentException();
        }
    }
}