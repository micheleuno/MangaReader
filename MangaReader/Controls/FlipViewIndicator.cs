using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MangaReader.Controls
{
    class FlipViewIndicator:ListBox
    {
        public FlipViewIndicator()
        {
            this.DefaultStyleKey = typeof(FlipViewIndicator);
        }
        public FlipView FlipView
        {
            get { return (FlipView)GetValue(FlipViewProperty);}
            set { SetValue(FlipViewProperty, value); }
        }

        public DependencyProperty FlipViewProperty =
            DependencyProperty.Register("FlipView", typeof(FlipView), typeof(FlipViewIndicator), new PropertyMetadata(null, (depobj, args) => 
            {

            FlipViewIndicator fvi = ( FlipViewIndicator)depobj;
         

            FlipView fv = (FlipView)args.NewValue;
          /*  fv.SelectionChanged += (s, e) =>           
            {
                fvi.ItemsSource = fv.ItemsSource;
            };
            fvi.ItemsSource = fv.ItemsSource;*/
            Binding eb = new Binding();
            eb.Mode = BindingMode.TwoWay;
            eb.Source = fv;
            eb.Path = new PropertyPath("SelectedItem");


        }));
    }
}
