using System;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace NetVips.Benchmarks.ImageSharp;

/// <summary>
/// Defines a processor that uses a 2 dimensional matrix to perform convolution against an image.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConvolutionProcessor"/> class.
/// </remarks>
/// <param name="kernelXY">The 2d gradient operator.</param>
/// <param name="preserveAlpha">Whether the convolution filter is applied to alpha as well as the color channels.</param>
public sealed class ConvolutionProcessor(in DenseMatrix<float> kernelXY, bool preserveAlpha) : IImageProcessor
{

    /// <summary>
    /// Gets the 2d gradient operator.
    /// </summary>
    public DenseMatrix<float> KernelXY { get; } = kernelXY;

    /// <summary>
    /// Gets a value indicating whether the convolution filter is applied to alpha as well as the color channels.
    /// </summary>
    public bool PreserveAlpha { get; } = preserveAlpha;

    /// <inheritdoc />
    public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration,
        Image<TPixel> source,
        Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
    {
        var type = Type.GetType(
            "SixLabors.ImageSharp.Processing.Processors.Convolution.ConvolutionProcessor`1, SixLabors.ImageSharp");
        Type[] typeArgs = [typeof(TPixel)];
        var genericType = type.MakeGenericType(typeArgs);
        Type[] parameterTypes =
        [
            configuration.GetType(), KernelXY.GetType().MakeByRefType(), PreserveAlpha.GetType(), source.GetType(),
            sourceRectangle.GetType()
        ];
        var ctor = genericType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
            parameterTypes, null);
        var instance =
            ctor.Invoke([configuration, KernelXY, PreserveAlpha, source, sourceRectangle]);

        return (IImageProcessor<TPixel>)instance;
    }
}