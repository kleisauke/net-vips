namespace NetVips.Benchmarks.ImageSharp
{
    using System;
    using System.Reflection;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing.Processors;

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
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration,
            Image<TPixel> source,
            Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            var type = Type.GetType(
                "SixLabors.ImageSharp.Processing.Processors.Convolution.ConvolutionProcessor`1, SixLabors.ImageSharp");
            Type[] typeArgs = { typeof(TPixel) };
            Type genericType = type.MakeGenericType(typeArgs);
            Type[] parameterTypes =
            {
                configuration.GetType(), KernelXY.GetType().MakeByRefType(), PreserveAlpha.GetType(), source.GetType(),
                sourceRectangle.GetType()
            };
            ConstructorInfo ctor = genericType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
                parameterTypes, null);
            object instance =
                ctor.Invoke(new object[] { configuration, KernelXY, PreserveAlpha, source, sourceRectangle });

            return (IImageProcessor<TPixel>)instance;
        }
    }
}