using System.Linq;
using NetVips.Internal;

namespace NetVips
{
    public sealed partial class Image : VipsObject
    {
        #region overloadable operators

        /// <summary>
        /// This operation calculates <paramref name="left"/> + <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator +(Image left, Image right) =>
            left.Call("add", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> + <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator +(Image left, double[] right) =>
            left.Call("linear", 1, right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> + <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator +(Image left, double right) =>
            left.Call("linear", 1, right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> + <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator +(Image left, int[] right) =>
            left.Call("linear", 1, right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> + <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator +(Image left, int right) =>
            left.Call("linear", 1, right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> - <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator -(Image left, Image right) =>
            left.Call("subtract", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> - <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator -(Image left, double[] right) =>
            left.Call("linear", 1, right.Select(x => x * -1).ToArray()) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> - <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator -(Image left, double right) =>
            left.Call("linear", 1, right * -1) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> - <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator -(Image left, int[] right) =>
            left.Call("linear", 1, right.Select(x => x * -1).ToArray()) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> - <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator -(Image left, int right) =>
            left.Call("linear", 1, right * -1) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> * <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator *(Image left, Image right) =>
           left.Call("multiply", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> * <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator *(Image left, double[] right) =>
            left.Call("linear", right, 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> * <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator *(Image left, double right) =>
            left.Call("linear", right, 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> * <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator *(Image left, int[] right) =>
            left.Call("linear", right, 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> * <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator *(Image left, int right) =>
            left.Call("linear", right, 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> / <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator /(Image left, Image right) =>
            left.Call("divide", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> / <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator /(Image left, double[] right) =>
            left.Call("linear", right.Select(x => 1.0 / x).ToArray(), 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> / <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator /(Image left, double right) =>
            left.Call("linear", 1.0 / right, 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> / <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator /(Image left, int[] right) =>
            left.Call("linear", right.Select(x => 1.0 / x).ToArray(), 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> / <paramref name="right"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator /(Image left, int right) =>
            left.Call("linear", 1.0 / right, 0) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> % <paramref name="right"/>
        /// (remainder after integer division).
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator %(Image left, Image right) =>
            left.Call("remainder", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> % <paramref name="right"/>
        /// (remainder after integer division).
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator %(Image left, double[] right) =>
            left.Call("remainder_const", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> % <paramref name="right"/>
        /// (remainder after integer division).
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator %(Image left, double right) =>
            left.Call("remainder_const", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> % <paramref name="right"/>
        /// (remainder after integer division).
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator %(Image left, int[] right) =>
            left.Call("remainder_const", right) as Image;

        /// <summary>
        /// This operation calculates <paramref name="left"/> % <paramref name="right"/>
        /// (remainder after integer division).
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator %(Image left, int right) =>
            left.Call("remainder_const", right) as Image;

        /// <summary>
        /// This operation computes the logical bitwise AND of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator &(Image left, Image right) =>
            left.Call("boolean", right, "and") as Image;

        /// <summary>
        /// This operation computes the logical bitwise AND of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator &(Image left, double[] right) =>
            left.Call("boolean_const", "and", right) as Image;

        /// <summary>
        /// This operation computes the logical bitwise AND of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator &(Image left, double right) =>
            left.Call("boolean_const", "and", right) as Image;

        /// <summary>
        /// This operation computes the logical bitwise AND of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator &(Image left, int right) =>
            left.Call("boolean_const", "and", right) as Image;

        /// <summary>
        /// This operation computes the logical bitwise AND of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator &(Image left, int[] right) =>
            left.Call("boolean_const", "and", right) as Image;

        /// <summary>
        /// This operation computes the bitwise OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator |(Image left, Image right) =>
            left.Call("boolean", right, "or") as Image;

        /// <summary>
        /// This operation computes the bitwise OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator |(Image left, double[] right) =>
            left.Call("boolean_const", "or", right) as Image;

        /// <summary>
        /// This operation computes the bitwise OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator |(Image left, double right) =>
            left.Call("boolean_const", "or", right) as Image;

        /// <summary>
        /// This operation computes the bitwise OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator |(Image left, int[] right) =>
            left.Call("boolean_const", "or", right) as Image;

        /// <summary>
        /// This operation computes the bitwise OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator |(Image left, int right) =>
            left.Call("boolean_const", "or", right) as Image;

        /// <summary>
        /// This operation computes the bitwise exclusive-OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator ^(Image left, Image right) =>
            left.Call("boolean", right, "eor") as Image;

        /// <summary>
        /// This operation computes the bitwise exclusive-OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator ^(Image left, double[] right) =>
           left.Call("boolean_const", "eor", right) as Image;

        /// <summary>
        /// This operation computes the bitwise exclusive-OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator ^(Image left, double right) =>
           left.Call("boolean_const", "eor", right) as Image;

        /// <summary>
        /// This operation computes the bitwise exclusive-OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator ^(Image left, int[] right) =>
           left.Call("boolean_const", "eor", right) as Image;

        /// <summary>
        /// This operation computes the bitwise exclusive-OR of its operands.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator ^(Image left, int right) =>
           left.Call("boolean_const", "eor", right) as Image;

        /// <summary>
        /// This operation shifts its first operand left by the number of bits specified by its second operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <<(Image left, int right) =>
            left.Call("boolean_const", "lshift", right) as Image;

        /// <summary>
        /// This operation shifts its first operand right by the number of bits specified by its second operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >>(Image left, int right) =>
            left.Call("boolean_const", "rshift", right) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Equal(Image right) =>
            this.Call("relational", right, "equal") as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Equal(double[] right) =>
           this.Call("relational_const", "equal", right) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Equal(double right) =>
           this.Call("relational_const", "equal", right) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Equal(int[] right) =>
           this.Call("relational_const", "equal", right) as Image;

        /// <summary>
        /// This operation compares two images on equality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image Equal(int right) =>
           this.Call("relational_const", "equal", right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image NotEqual(Image right) =>
            this.Call("relational", right, "noteq") as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image NotEqual(double right) =>
           this.Call("relational_const", "noteq", right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image NotEqual(double[] right) =>
           this.Call("relational_const", "noteq", right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image NotEqual(int[] right) =>
           this.Call("relational_const", "noteq", right) as Image;

        /// <summary>
        /// This operation compares two images on inequality.
        /// </summary>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public Image NotEqual(int right) =>
           this.Call("relational_const", "noteq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <(Image left, Image right) =>
            left.Call("relational", right, "less") as Image;

        /// <summary>
        /// This operation compares if the left operand is less than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <(Image left, double[] right) =>
           left.Call("relational_const", "less", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <(Image left, double right) =>
           left.Call("relational_const", "less", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <(Image left, int[] right) =>
           left.Call("relational_const", "less", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <(Image left, int right) =>
           left.Call("relational_const", "less", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >(Image left, Image right) =>
            left.Call("relational", right, "more") as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >(Image left, double[] right) =>
           left.Call("relational_const", "more", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >(Image left, double right) =>
           left.Call("relational_const", "more", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >(Image left, int[] right) =>
           left.Call("relational_const", "more", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >(Image left, int right) =>
           left.Call("relational_const", "more", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <=(Image left, Image right) =>
            left.Call("relational", right, "lesseq") as Image;

        /// <summary>
        /// This operation compares if the left operand is less than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <=(Image left, double[] right) =>
           left.Call("relational_const", "lesseq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <=(Image left, double right) =>
           left.Call("relational_const", "lesseq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <=(Image left, int[] right) =>
           left.Call("relational_const", "lesseq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is less than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator <=(Image left, int right) =>
           left.Call("relational_const", "lesseq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >=(Image left, Image right) =>
            left.Call("relational", right, "moreeq") as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >=(Image left, double[] right) =>
           left.Call("relational_const", "moreeq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >=(Image left, double right) =>
           left.Call("relational_const", "moreeq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >=(Image left, int[] right) =>
           left.Call("relational_const", "moreeq", right) as Image;

        /// <summary>
        /// This operation compares if the left operand is greater than or equal to the right operand.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>A new <see cref="Image"/></returns>
        public static Image operator >=(Image left, int right) =>
           left.Call("relational_const", "moreeq", right) as Image;

        #endregion
    }
}
