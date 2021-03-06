﻿using System;
using TwistedLogik.Nucleus;

namespace TwistedLogik.Gluon
{
    public static unsafe partial class gl
    {
        [MonoNativeFunctionWrapper]
        private delegate void glNamedBufferDataEXTDelegate(uint buffer, IntPtr size, IntPtr data, uint usage);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glNamedBufferDataEXTDelegate glNamedBufferDataEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glNamedBufferSubDataEXTDelegate(uint buffer, IntPtr offset, IntPtr size, IntPtr data);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glNamedBufferSubDataEXTDelegate glNamedBufferSubDataEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate uint glCheckNamedFramebufferStatusEXTDelegate(uint framebuffer, uint target);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glCheckNamedFramebufferStatusEXTDelegate glCheckNamedFramebufferStatusEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glNamedFramebufferTextureEXTDelegate(uint framebuffer, uint attachment, uint texture, int level);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glNamedFramebufferTextureEXTDelegate glNamedFramebufferTextureEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glNamedFramebufferRenderbufferEXTDelegate(uint framebuffer, uint attachment, uint renderbuffertarget, uint renderbuffer);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glNamedFramebufferRenderbufferEXTDelegate glNamedFramebufferRenderbufferEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glFramebufferDrawBufferEXTDelegate(uint framebuffer, uint mode);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glFramebufferDrawBufferEXTDelegate glFramebufferDrawBufferEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glFramebufferDrawBuffersEXTDelegate(uint framebuffer, int n, IntPtr bufs);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glFramebufferDrawBuffersEXTDelegate glFramebufferDrawBuffersEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glTextureParameteriEXTDelegate(uint texture, uint target, uint pname, int param);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glTextureParameteriEXTDelegate glTextureParameteriEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glTextureImage2DEXTDelegate(uint texture, uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glTextureImage2DEXTDelegate glTextureImage2DEXT = null;

        [MonoNativeFunctionWrapper]
        private delegate void glTextureSubImage2DEXTDelegate(uint texture, uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, IntPtr pixels);
        [Require(Extension = "GL_EXT_direct_state_access")]
        private static readonly glTextureSubImage2DEXTDelegate glTextureSubImage2DEXT = null;
    }
}
