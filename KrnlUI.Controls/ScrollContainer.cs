using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace KrnlUI.Controls;

public partial class ScrollContainer : UserControl, IComponentConnector
{
	private int AccPad;

	public int ScrollLength = 40;

	private Storyboard ScrollStoryboard = new Storyboard();

	private ThicknessAnimation ScrollPosAnimator = new ThicknessAnimation
	{
		Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
		DecelerationRatio = 1.0
	};

	public List<UIElement> Children
	{
		get
		{
			List<UIElement> list = new List<UIElement>();
			foreach (UIElement child in ((Grid)base.Content).Children)
			{
				list.Add(child);
			}
			return list;
		}
		set
		{
			foreach (UIElement item in value)
			{
				((Grid)base.Content).Children.Add(item);
			}
		}
	}

	public ScrollContainer()
	{
		InitializeComponent();
		Storyboard.SetTargetProperty(ScrollPosAnimator, new PropertyPath("Margin"));
		ScrollStoryboard.Children.Add(ScrollPosAnimator);
	}

	private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (VisualChildrenCount == 1 && base.IsLoaded)
		{
			ScrollViewer scrollViewer = (ScrollViewer)base.Content;
			Storyboard.SetTarget(ScrollPosAnimator, scrollViewer);
			ScrollStoryboard.Stop();
			MessageBox.Show(scrollViewer.ViewportHeight.ToString());
			int num = ScrollLength * (e.Delta / 120);
			AccPad += num;
			if (AccPad > 0)
			{
				ScrollPosAnimator.To = new Thickness(0.0, 0.0, 0.0, 0.0);
				AccPad = 0;
			}
			else
			{
				ScrollPosAnimator.To = new Thickness(0.0, AccPad, 0.0, 0.0);
			}
			ScrollStoryboard.Begin();
		}
	}
}
