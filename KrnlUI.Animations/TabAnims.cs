using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KrnlUI.Animations;

internal class TabAnims
{
	private static int animTime = 20;

	private static int smooth = 5;

	public static void NewTabEnter(object sender, MouseEventArgs e)
	{
		((Label)sender).Foreground = new SolidColorBrush(Color.FromRgb(175, 175, 175));
	}

	public static void NewTabLeave(object sender, MouseEventArgs e)
	{
		((Label)sender).Foreground = new SolidColorBrush(Color.FromRgb(122, 122, 122));
	}

	public static void AnimateNewTab(object sender)
	{
		_ = (Grid)sender;
	}

	private static async void AnimEnter(object sender, Color color, Color secondary)
	{
		StackPanel target = (StackPanel)sender;
		if ((bool)target.Tag)
		{
			return;
		}
		target.Tag = true;
		Color color2 = Color.FromRgb(39, 39, 39);
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
}
