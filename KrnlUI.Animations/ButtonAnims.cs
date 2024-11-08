using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KrnlUI.Animations;

internal class ButtonAnims
{
	private static int animTime = 20;

	private static int smooth = 5;

	public static void ButtonExitEnter(object sender, MouseEventArgs e)
	{
		AnimEnter(sender, Color.FromRgb(231, 16, 34), Color.FromRgb(200, 200, 200));
	}

	public static void ButtonExitLeave(object sender, MouseEventArgs e)
	{
		AnimLeave(sender, Color.FromRgb(34, 34, 34), Color.FromRgb(122, 122, 122));
	}

	public static void ButtonExitDown(object sender, MouseEventArgs e)
	{
		AnimDown(sender, Color.FromRgb(194, 16, 29));
	}

	public static void ButtonExitUp(object sender, MouseEventArgs e)
	{
		AnimUp(sender, Color.FromRgb(231, 16, 34));
	}

	public static void ButtonTopEnter(object sender, MouseEventArgs e)
	{
		AnimEnter(sender, Color.FromRgb(57, 57, 57), Color.FromRgb(200, 200, 200));
	}

	public static void ButtonTopLeave(object sender, MouseEventArgs e)
	{
		AnimLeave(sender, Color.FromRgb(34, 34, 34), Color.FromRgb(122, 122, 122));
	}

	public static void ButtonTopDown(object sender, MouseEventArgs e)
	{
		AnimDown(sender, Color.FromRgb(74, 74, 74));
	}

	public static void ButtonTopUp(object sender, MouseEventArgs e)
	{
		AnimUp(sender, Color.FromRgb(57, 57, 57));
	}

	private static async void AnimEnter(object sender, Color color, Color secondary)
	{
		StackPanel target = (StackPanel)sender;
		if ((bool)target.Tag)
		{
			return;
		}
		target.Tag = true;
		Color color2 = Color.FromRgb(34, 34, 34);
		int yield = animTime / smooth;
		byte cr = (byte)((color.R - color2.R) / smooth);
		byte cg = (byte)((color.G - color2.G) / smooth);
		byte cb = (byte)((color.B - color2.B) / smooth);
		byte r = color2.R;
		byte g = color2.G;
		byte b = color2.B;
		((Label)target.Children[0]).Foreground = new SolidColorBrush(secondary);
		for (int i = 0; i < smooth; i++)
		{
			if ((bool)target.Tag)
			{
				r += cr;
				g += cg;
				b += cb;
				target.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
				await Task.Delay(yield);
				continue;
			}
			target.Background = new SolidColorBrush(color);
			break;
		}
	}

	private static async void AnimLeave(object sender, Color color, Color secondary)
	{
		StackPanel target = (StackPanel)sender;
		target.Tag = false;
		Color color2 = Color.FromRgb(34, 34, 34);
		int yield = animTime / smooth;
		byte cr = (byte)((color.R - color2.R) / smooth);
		byte cg = (byte)((color.G - color2.G) / smooth);
		byte cb = (byte)((color.B - color2.B) / smooth);
		byte r = color2.R;
		byte g = color2.G;
		byte b = color2.B;
		((Label)target.Children[0]).Foreground = new SolidColorBrush(secondary);
		for (int i = 0; i < smooth; i++)
		{
			if (!(bool)target.Tag)
			{
				r += cr;
				g += cg;
				b += cb;
				target.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
				await Task.Delay(yield);
				continue;
			}
			target.Background = new SolidColorBrush(color);
			break;
		}
	}

	private static async void AnimDown(object sender, Color color)
	{
		StackPanel target = (StackPanel)sender;
		if (!(bool)target.Tag)
		{
			return;
		}
		target.Tag = false;
		Color color2 = ((SolidColorBrush)target.Background).Color;
		int yield = animTime / smooth;
		byte cr = (byte)((color.R - color2.R) / smooth);
		byte cg = (byte)((color.G - color2.G) / smooth);
		byte cb = (byte)((color.B - color2.B) / smooth);
		byte r = color2.R;
		byte g = color2.G;
		byte b = color2.B;
		for (int i = 0; i < smooth; i++)
		{
			if (!(bool)target.Tag)
			{
				r += cr;
				g += cg;
				b += cb;
				target.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
				await Task.Delay(yield);
				continue;
			}
			target.Background = new SolidColorBrush(color);
			break;
		}
	}

	private static async void AnimUp(object sender, Color color)
	{
		StackPanel target = (StackPanel)sender;
		if ((bool)target.Tag)
		{
			return;
		}
		target.Tag = true;
		Color color2 = ((SolidColorBrush)target.Background).Color;
		int yield = animTime / smooth;
		byte cr = (byte)((color.R - color2.R) / smooth);
		byte cg = (byte)((color.G - color2.G) / smooth);
		byte cb = (byte)((color.B - color2.B) / smooth);
		byte r = color2.R;
		byte g = color2.G;
		byte b = color2.B;
		for (int i = 0; i < smooth; i++)
		{
			if ((bool)target.Tag)
			{
				r += cr;
				g += cg;
				b += cb;
				target.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
				await Task.Delay(yield);
				continue;
			}
			target.Background = new SolidColorBrush(color);
			break;
		}
	}
}
