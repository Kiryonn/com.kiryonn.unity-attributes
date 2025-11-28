using UnityEngine;
using UnityEngine.UIElements;
namespace Kiryonn.UnityAttributes
{
	public static class StyleExtension
	{
		#region Padding
		/// <summary>Applies paddings using css rules.</summary>
		public static void Padding(this IStyle style, float value)
		{
			style.paddingTop = value;
			style.paddingRight = value;
			style.paddingBottom = value;
			style.paddingLeft = value;
		}
		/// <summary>Applies paddings using css rules.</summary>
		public static void Padding(this IStyle style, float vertical, float horizontal)
		{
			style.paddingTop = vertical;
			style.paddingRight = horizontal;
			style.paddingBottom = vertical;
			style.paddingLeft = horizontal;
		}
		/// <summary>Applies paddings using css rules.</summary>
		public static void Padding(this IStyle style, float top, float horizontal, float bottom)
		{
			style.paddingTop = top;
			style.paddingRight = horizontal;
			style.paddingBottom = bottom;
			style.paddingLeft = horizontal;
		}
		/// <summary>Applies paddings using css rules.</summary>
		public static void Padding(this IStyle style, float top, float right, float bottom, float left)
		{
			style.paddingTop = top;
			style.paddingRight = right;
			style.paddingBottom = bottom;
			style.paddingLeft = left;
		}
		#endregion Padding

		#region Margin
		/// <summary>Applies margins using css rules.</summary>
		public static void Margin(this IStyle style, float value)
		{
			style.marginTop = value;
			style.marginRight = value;
			style.marginBottom = value;
			style.marginLeft = value;
		}
		/// <summary>Applies margins using css rules.</summary>
		public static void Margin(this IStyle style, float vertical, float horizontal)
		{
			style.marginTop = vertical;
			style.marginRight = horizontal;
			style.marginBottom = vertical;
			style.marginLeft = horizontal;
		}
		/// <summary>Applies margins using css rules.</summary>
		public static void Margin(this IStyle style, float top, float horizontal, float bottom)
		{
			style.marginTop = top;
			style.marginRight = horizontal;
			style.marginBottom = bottom;
			style.marginLeft = horizontal;
		}
		/// <summary>Applies margins using css rules.</summary>
		public static void Margin(this IStyle style, float top, float right, float bottom, float left)
		{
			style.marginTop = top;
			style.marginRight = right;
			style.marginBottom = bottom;
			style.marginLeft = left;
		}
		#endregion Margin

		#region Border Width
		/// <summary>Applies border widths using css rules.</summary>
		public static void BorderWidth(this IStyle style, float value)
		{
			style.borderTopWidth = value;
			style.borderRightWidth = value;
			style.borderBottomWidth = value;
			style.borderLeftWidth = value;
		}
		/// <summary>Applies border widths using css rules.</summary>
		public static void BorderWidth(this IStyle style, float vertical, float horizontal)
		{
			style.borderTopWidth = vertical;
			style.borderRightWidth = horizontal;
			style.borderBottomWidth = vertical;
			style.borderLeftWidth = horizontal;
		}
		/// <summary>Applies border widths using css rules.</summary>
		public static void BorderWidth(this IStyle style, float top, float horizontal, float bottom)
		{
			style.borderTopWidth = top;
			style.borderRightWidth = horizontal;
			style.borderBottomWidth = bottom;
			style.borderLeftWidth = horizontal;
		}
		/// <summary>Applies border widths using css rules.</summary>
		public static void BorderWidth(this IStyle style, float top, float right, float bottom, float left)
		{
			style.borderTopWidth = top;
			style.borderRightWidth = right;
			style.borderBottomWidth = bottom;
			style.borderLeftWidth = left;
		}
		#endregion Border Width

		#region Border Color

		/// <summary>Applies border colors using css rules.</summary>
		public static void BorderColor(this IStyle style, Color value)
		{
			style.borderTopColor = value;
			style.borderRightColor = value;
			style.borderBottomColor = value;
			style.borderLeftColor = value;
		}
		/// <summary>Applies border colors using css rules.</summary>
		public static void BorderColor(this IStyle style, Color vertical, Color horizontal)
		{
			style.borderTopColor = vertical;
			style.borderRightColor = horizontal;
			style.borderBottomColor = vertical;
			style.borderLeftColor = horizontal;
		}
		/// <summary>Applies border colors using css rules.</summary>
		public static void BorderColor(this IStyle style, Color top, Color horizontal, Color bottom)
		{
			style.borderTopColor = top;
			style.borderRightColor = horizontal;
			style.borderBottomColor = bottom;
			style.borderLeftColor = horizontal;
		}
		/// <summary>Applies border colors using css rules.</summary>
		public static void BorderColor(this IStyle style, Color top, Color right, Color bottom, Color left)
		{
			style.borderTopColor = top;
			style.borderRightColor = right;
			style.borderBottomColor = bottom;
			style.borderLeftColor = left;
		}
		#endregion Border Color
	}
}