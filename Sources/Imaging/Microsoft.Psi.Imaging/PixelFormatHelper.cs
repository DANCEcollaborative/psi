﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Psi.Imaging
{
    using System;

    /// <summary>
    /// Set of static functions for manipulating pixel formats.
    /// </summary>
    internal static class PixelFormatHelper
    {
        /// <summary>
        /// Converts from a system pixel format into a Psi.Imaging pixel format.
        /// </summary>
        /// <param name="pixelFormat">System pixel format to be converted.</param>
        /// <returns>Psi.Imaging pixel format that matches the specified system pixel format.</returns>
        internal static PixelFormat FromSystemPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                return PixelFormat.BGR_24bpp;
            }

            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
            {
                return PixelFormat.BGRX_32bpp;
            }

            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                return PixelFormat.Gray_8bpp;
            }

            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)
            {
                return PixelFormat.Gray_16bpp;
            }

            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                return PixelFormat.BGRA_32bpp;
            }

            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format64bppArgb)
            {
                return PixelFormat.RGBA_64bpp;
            }

            throw new NotSupportedException($"The {pixelFormat} pixel format is not currently supported by {nameof(Microsoft.Psi.Imaging)}.");
        }

        /// <summary>
        /// Converts from a Psi.Imaging PixelFormat to a System.Drawing.Imaging.PixelFormat.
        /// </summary>
        /// <param name="pixelFormat">Pixel format to convert.</param>
        /// <returns>The system pixel format that corresponds to the Psi.Imaging pixel format.</returns>
        internal static System.Drawing.Imaging.PixelFormat ToSystemPixelFormat(PixelFormat pixelFormat)
        {
            return pixelFormat switch
            {
                PixelFormat.BGR_24bpp => System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                PixelFormat.BGRX_32bpp => System.Drawing.Imaging.PixelFormat.Format32bppRgb,
                PixelFormat.Gray_8bpp => System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                PixelFormat.Gray_16bpp => System.Drawing.Imaging.PixelFormat.Format16bppGrayScale,
                PixelFormat.BGRA_32bpp => System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                PixelFormat.RGBA_64bpp => System.Drawing.Imaging.PixelFormat.Format64bppArgb,
                PixelFormat.Undefined =>
                    throw new InvalidOperationException(
                        $"Cannot convert {nameof(PixelFormat.Undefined)} pixel format to {nameof(System.Drawing.Imaging.PixelFormat)}."),
                _ => throw new Exception("Unknown pixel format."),
            };
        }

        /// <summary>
        /// Returns number of bytes/pixel for the specified pixel format.
        /// </summary>
        /// <param name="pixelFormat">Pixel format for which to determine number of bytes.</param>
        /// <returns>Number of bytes in each pixel of the specified format.</returns>
        internal static int GetBytesPerPixel(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Gray_8bpp:
                    return 1;

                case PixelFormat.Gray_16bpp:
                    return 2;

                case PixelFormat.BGR_24bpp:
                    return 3;

                case PixelFormat.BGRX_32bpp:
                case PixelFormat.BGRA_32bpp:
                    return 4;

                case PixelFormat.RGBA_64bpp:
                    return 8;

                case PixelFormat.Undefined:
                    return 0;

                default:
                    throw new ArgumentException("Unknown pixel format");
            }
        }
    }
}
