﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Psi.Imaging
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using Microsoft.Psi.Common;
    using Microsoft.Psi.Serialization;

    /// <summary>
    /// Represents a depth image, stored in unmanaged memory.
    /// </summary>
    /// <remarks>Using this class it is possible as to allocate a new depth image in unmanaged memory,
    /// as to just wrap provided pointer to unmanaged memory, where an image is stored.</remarks>
    [Serializer(typeof(DepthImage.CustomSerializer))]
    public class DepthImage : ImageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepthImage"/> class.
        /// </summary>
        /// <param name="unmanagedBuffer">The unmanaged array containing the image.</param>
        /// <param name="width">Depth image width in pixels.</param>
        /// <param name="height">Depth image height in pixels.</param>
        /// <param name="stride">Depth image stride (line size in bytes).</param>
        /// <remarks><para><note>Using this constructor, make sure all specified image attributes are correct
        /// and correspond to unmanaged memory buffer. If some attributes are specified incorrectly,
        /// this may lead to exceptions working with the unmanaged memory.</note></para></remarks>
        public DepthImage(UnmanagedBuffer unmanagedBuffer, int width, int height, int stride)
            : base(unmanagedBuffer, width, height, stride, PixelFormat.Gray_16bpp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthImage"/> class.
        /// </summary>
        /// <param name="imageData">Pointer to image data in unmanaged memory.</param>
        /// <param name="width">Depth image width in pixels.</param>
        /// <param name="height">Depth image height in pixels.</param>
        /// <param name="stride">Depth image stride (line size in bytes).</param>
        /// <remarks><para><note>Using this constructor, make sure all specified image attributes are correct
        /// and correspond to unmanaged memory buffer. If some attributes are specified incorrectly,
        /// this may lead to exceptions working with the unmanaged memory.</note></para></remarks>
        public DepthImage(IntPtr imageData, int width, int height, int stride)
            : base(imageData, width, height, stride, PixelFormat.Gray_16bpp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthImage"/> class.
        /// </summary>
        /// <param name="width">Depth image width in pixels.</param>
        /// <param name="height">Depth image height in pixels.</param>
        public DepthImage(int width, int height)
            : base(width, height, PixelFormat.Gray_16bpp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthImage"/> class.
        /// </summary>
        /// <param name="width">Depth image width in pixels.</param>
        /// <param name="height">Depth image height in pixels.</param>
        /// <param name="stride">Depth image stride (line size in bytes).</param>
        /// <remarks><para><note>Using this constructor, make sure all specified image attributes are correct
        /// and correspond to unmanaged memory buffer. If some attributes are specified incorrectly,
        /// this may lead to exceptions working with the unmanaged memory.</note></para></remarks>
        public DepthImage(int width, int height, int stride)
            : base(width, height, stride, PixelFormat.Gray_16bpp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthImage"/> class.
        /// </summary>
        /// <param name="bitmapData">Locked bitmap data.</param>
        /// <param name="makeCopy">Indicates whether a copy is made (default is false).</param>
        /// <remarks>
        /// <para>When the <paramref name="makeCopy"/> parameter is false (default), the depth image simply wraps
        /// the bitmap data. As such, the bitmap data must stay locked for the duration of using the <see cref="DepthImage"/> object.
        /// </para>
        /// <para>If the <paramref name="makeCopy"/> parameter is set to true, a copy of the bitmap
        /// data is made, and the bitmap data can be released right after the <see cref="DepthImage"/> has been constructed.
        /// </para>
        /// </remarks>
        public DepthImage(BitmapData bitmapData, bool makeCopy = false)
            : base(bitmapData, makeCopy)
        {
            CheckPixelFormat(bitmapData.PixelFormat);
        }

        /// <summary>
        /// Create a new <see cref="DepthImage"/> from a specified bitmap.
        /// </summary>
        /// <param name="bitmap">A bitmap to create the depth image from.</param>
        /// <returns>A new depth image, which contains a copy of the specified bitmap.</returns>
        public static DepthImage CreateFrom(Bitmap bitmap)
        {
            CheckPixelFormat(bitmap.PixelFormat);

            DepthImage depthImage = null;
            BitmapData sourceData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            try
            {
                depthImage = new DepthImage(sourceData, true);
            }
            finally
            {
                bitmap.UnlockBits(sourceData);
            }

            return depthImage;
        }

        /// <summary>
        /// Copies the depth image contents from a specified source locked bitmap data.
        /// </summary>
        /// <param name="bitmapData">Source locked bitmap data.</param>
        /// <remarks><para>The method copies data from the specified bitmap into the depth image.
        /// The depth image must be allocated and must have the same size as the specified
        /// bitmap data.</para></remarks>
        public void CopyFrom(BitmapData bitmapData)
        {
            CheckPixelFormat(bitmapData.PixelFormat);
            int numBytes = bitmapData.Height * bitmapData.Stride;
            if (numBytes > this.UnmanagedBuffer.Size)
            {
                throw new InvalidOperationException("Buffer too small.");
            }

            this.UnmanagedBuffer.CopyFrom(bitmapData.Scan0, numBytes);
        }

        /// <summary>
        /// Copies the depth image contents from a specified bitmap.
        /// </summary>
        /// <param name="bitmap">A bitmap to copy from.</param>
        /// <remarks><para>The method copies data from the specified bitmap into the image.
        /// The image must be allocated and must have the same size.</para></remarks>
        public void CopyFrom(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, this.Width, this.Height),
                ImageLockMode.ReadWrite,
                PixelFormatHelper.ToSystemPixelFormat(this.PixelFormat));
            try
            {
                int numBytes = bitmapData.Height * bitmapData.Stride;
                if (numBytes > this.UnmanagedBuffer.Size)
                {
                    throw new InvalidOperationException("Buffer too small.");
                }

                this.UnmanagedBuffer.CopyFrom(bitmapData.Scan0, numBytes);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the depth image from a specified source depth image of the same size.
        /// </summary>
        /// <param name="source">Source depth image to copy the depth image from.</param>
        /// <remarks><para>The method copies the current depth image from the specified source depth image.
        /// The size of the images must be the same.</para></remarks>
        public void CopyFrom(DepthImage source)
        {
            source.CopyTo(this);
        }

        /// <summary>
        /// Copies the depth image from a specified source image of the same size and <see cref="PixelFormat.Gray_16bpp"/> format.
        /// </summary>
        /// <param name="source">Source image to copy the depth image from.</param>
        /// <remarks><para>The method copies the current depth image from the specified source image.
        /// The size of the images must be the same, and the source image must have <see cref="PixelFormat.Gray_16bpp"/> format.</para></remarks>
        public void CopyFrom(Image source)
        {
            source.CopyTo(this);
        }

        /// <summary>
        /// Decodes a specified encoded depth image with a specified decoder into the current depth image.
        /// </summary>
        /// <param name="encodedDepthImage">The encoded depth image to decode.</param>
        /// <param name="depthImageDecoder">The depth image decoder to use.</param>
        /// <remarks>The depth image width, height and pixel format must match. The method should not be called concurrently.</remarks>
        public void DecodeFrom(EncodedDepthImage encodedDepthImage, IDepthImageFromStreamDecoder depthImageDecoder)
        {
            if (encodedDepthImage.Width != this.Width || encodedDepthImage.Height != this.Height || encodedDepthImage.PixelFormat != this.PixelFormat)
            {
                throw new InvalidOperationException("Cannot decode from an encoded depth image that has a different width, height, or pixel format.");
            }

            depthImageDecoder.DecodeFromStream(encodedDepthImage.ToStream(), this);
        }

        /// <summary>
        /// Encodes the depth image using a specified encoder.
        /// </summary>
        /// <param name="depthImageEncoder">The depth image encoder to use.</param>
        /// <returns>A new, corresponding encoded depth image.</returns>
        public EncodedDepthImage Encode(IDepthImageToStreamEncoder depthImageEncoder)
        {
            var encodedDepthImage = new EncodedDepthImage(this.Width, this.Height);
            encodedDepthImage.EncodeFrom(this, depthImageEncoder);
            return encodedDepthImage;
        }

        /// <summary>
        /// Copies the depth image into a target depth image of the same size.
        /// </summary>
        /// <param name="target">Target depth image to copy this depth image to.</param>
        /// <remarks><para>The method copies the current depth image into the specified depth image.
        /// The size of the images must be the same.</para></remarks>
        public void CopyTo(DepthImage target)
        {
            this.CopyTo(target.ImageData, target.Width, target.Height, target.Stride, target.PixelFormat);
        }

        /// <summary>
        /// Copies the depth image into a target image of the same size.
        /// </summary>
        /// <param name="target">Target image to copy this depth image to.</param>
        /// <remarks><para>The method copies the current depth image into the specified image.
        /// The size of the images must be the same. The method implements a translation of pixel formats.</para></remarks>
        public void CopyTo(Image target)
        {
            this.CopyTo(target.ImageData, target.Width, target.Height, target.Stride, target.PixelFormat);
        }

        /// <summary>
        /// Sets a pixel in the depth image.
        /// </summary>
        /// <param name="x">Pixel's X coordinate.</param>
        /// <param name="y">Pixel's Y coordinate.</param>
        /// <param name="gray">Gray value to set pixel to.</param>
        public void SetPixel(int x, int y, ushort gray)
        {
            if (x < 0 || x >= this.Width)
            {
                throw new ArgumentException("X coordinate is outside bounds", nameof(x));
            }

            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentException("Y coordinate is outside bounds", nameof(y));
            }

            unsafe
            {
                byte* src = (byte*)this.ImageData.ToPointer();
                int pixelOffset = x * this.BitsPerPixel / 8 + y * this.Stride;
                *(ushort*)(src + pixelOffset) = gray;
            }
        }

        /// <summary>
        /// Gets the value of a pixel in the depth image.
        /// </summary>
        /// <param name="x">Pixel's X coordinate.</param>
        /// <param name="y">Pixel's Y coordinate.</param>
        /// <returns>The value of the pixel at the specified coordinates.</returns>
        public ushort GetPixel(int x, int y)
        {
            if (x < 0 || x >= this.Width)
            {
                throw new ArgumentException("X coordinate is outside bounds", nameof(x));
            }

            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentException("Y coordinate is outside bounds", nameof(y));
            }

            unsafe
            {
                return *(ushort*)((byte*)this.ImageData.ToPointer() + y * this.Stride + x * this.BitsPerPixel / 8);
            }
        }

        /// <summary>
        /// Gets the range of values in the depth image.
        /// </summary>
        /// <returns>A tuple describing the range of values in the depth image.</returns>
        public (ushort, ushort) GetPixelRange()
        {
            ushort minRange = 65535;
            ushort maxRange = 0;

            unsafe
            {
                byte* src = (byte*)this.ImageData.ToPointer();
                for (int y = 0; y < this.Height; y++)
                {
                    var strideOffset = y * this.Stride;
                    for (int x = 0; x < this.Width; x++)
                    {
                        int pixelOffset = (x << 1) + strideOffset;
                        var value = *(ushort*)(src + pixelOffset);

                        if (value < minRange)
                        {
                            minRange = value;
                        }

                        if (value > maxRange)
                        {
                            maxRange = value;
                        }
                    }
                }
            }

            return (minRange, maxRange);
        }

        /// <inheritdoc/>
        public override ImageBase CreateEmptyOfSameSize()
        {
            return new DepthImage(this.Width, this.Height);
        }

        private static void CheckPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            if (pixelFormat != System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)
            {
                throw new InvalidOperationException(
                    $"Depth images can only be constructed from bitmaps with {nameof(System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)} format.");
            }
        }

        /// <summary>
        /// Custom serializer used for reading/writing depth images.
        /// </summary>
        public class CustomSerializer : ImageBase.CustomSerializer<DepthImage>
        {
            private static IDepthImageCompressor depthImageCompressor = null;

            /// <summary>
            /// Configure the type of compression to use when serializing depth images. Default is no compression.
            /// </summary>
            /// <param name="depthImageCompressor">Compressor to be used.</param>
            public static void ConfigureCompression(IDepthImageCompressor depthImageCompressor)
            {
                CustomSerializer.depthImageCompressor = depthImageCompressor;
            }

            /// <inheritdoc/>
            public override void Serialize(BufferWriter writer, DepthImage instance, SerializationContext context)
            {
                DepthCompressionMethod depthCompressionMethod = (depthImageCompressor == null) ? DepthCompressionMethod.None : depthImageCompressor.DepthCompressionMethod;
                Serializer.Serialize(writer, depthCompressionMethod, context);
                if (depthCompressionMethod == DepthCompressionMethod.None)
                {
                    base.Serialize(writer, instance, context);
                }
                else
                {
                    depthImageCompressor.Serialize(writer, instance, context);
                }
            }

            /// <inheritdoc/>
            public override void Deserialize(BufferReader reader, ref DepthImage target, SerializationContext context)
            {
                var depthCompressionMethod = DepthCompressionMethod.None;
                if (this.Schema.Version >= 4)
                {
                    Serializer.Deserialize(reader, ref depthCompressionMethod, context);
                }

                if (depthCompressionMethod == DepthCompressionMethod.None)
                {
                    base.Deserialize(reader, ref target, context);
                }
                else
                {
                    depthImageCompressor.Deserialize(reader, ref target, context);
                }
            }
        }
    }
}
