﻿using AppKit;

namespace UltravioletSample.Sample4_ContentManagement
{
    partial class Game
    {
        partial void PlatformSpecificInitialization()
        {
            NSApplication.Init();
        }
    }
}

