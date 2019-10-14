using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Processors.Convolution
{
    using System;
    using System.Reflection;
    using PixelFormats;
    using Primitives;

    /// <summary>
    /// Defines a processor that uses a 2 dimensional matrix to perform convolution against an image.
    /// </summary>
    public sealed class ConvolutionProcessor : IImageProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvolutionProcessor"/> class.
        /// </summary>
        /// <param name="kernelXY">The 2d gradient operator.</param>
        /// <param name="preserveAlpha">Whether the convolution filter is applied to alpha as well as the color channels.</param>
        public ConvolutionProcessor(in DenseMatrix<float> kernelXY, bool preserveAlpha)
        {
            KernelXY = kernelXY;
            PreserveAlpha = preserveAlpha;
        }

        /// <summary>
        /// Gets the 2d gradient operator.
        /// </summary>
        public DenseMatrix<float> KernelXY { get; }

        /// <summary>
        /// Gets a value indicating whether the convolution filter is applied to alpha as well as the color channels.
        /// </summary>
        public bool PreserveAlpha { get; }

        /// <inheritdoc />
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source,
            Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            var type = Type.GetType(
                "SixLabors.ImageSharp.Processing.Processors.Convolution.ConvolutionProcessor`1, SixLabors.ImageSharp");
            Type[] typeArgs = { typeof(TPixel) };
            Type genericType = type.MakeGenericType(typeArgs);
            Type[] parameterTypes =
            {
                KernelXY.GetType().MakeByRefType(), PreserveAlpha.GetType(), source.GetType(), sourceRectangle.GetType()
            };
            ConstructorInfo ctor = genericType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
                parameterTypes, null);
            object instance = ctor.Invoke(new object[] { KernelXY, PreserveAlpha, source, sourceRectangle });

            return (IImageProcessor<TPixel>)instance;
        }
    }
}