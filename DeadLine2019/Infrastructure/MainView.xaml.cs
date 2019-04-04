namespace DeadLine2019.Infrastructure
{
    using System.Windows;
    using System.Windows.Data;

    using Microsoft.Msagl.Drawing;
    using Microsoft.Msagl.WpfGraphControl;

    public partial class MainView
    {
        public GraphViewer GraphViewer { get; set; }

        public MainView()
        {
            InitializeComponent();

            GraphPanel.ClipToBounds = true;

            GraphViewer = new GraphViewer();
            GraphViewer.BindToPanel(GraphPanel);
            GraphViewer.Graph = new Graph();

            Loaded += (sender, args) => SetupBinding();
        }

        private void SetupBinding()
        {
            var binding = new Binding
            {
                Source = DataContext,
                Path = new PropertyPath(nameof(GraphViewer.Graph)),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(GraphViewer, GraphViewer.GraphProperty, binding);
        }
    }
}
