﻿using System;
using System.Runtime.InteropServices;
using TwistedLogik.Nucleus;

#pragma warning disable 1591

namespace TwistedLogik.Ultraviolet.SDL2.Native
{
    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_QuitEvent
    {
        public UInt32 type;
        public UInt32 timestamp;
    }
}
