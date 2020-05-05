using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kohonen.ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NeuralNetwork neural;

        //here we have just 2 attributes, X and Y=>neurons should have the same count of inputs
        const int INPUTS = 2;
        const int GRID_WIDTH = 300;

        private bool learned;

        private ObjectClassVisual dataVisual;
        private MapVisual mapVisual;
        enum ProcessMode
        {
            Learn,
            LearnAndProcess,
            ProcessLearned
        }

        private ProcessMode Mode { get; set; }
        public MainWindow()
        {
            InitializeComponent();

        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            dataVisual.Clear();
        }

        private void classifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataVisual.Objects.Count == 0)
            {
                MessageBox.Show("Необходимо создать набор объектов!");
                return;
            }
            if (Mode == ProcessMode.Learn || Mode == ProcessMode.LearnAndProcess)
            {
                neural = new NeuralNetwork(mapVisual.Dimension, INPUTS, dataVisual.Objects.Count, 
                    GRID_WIDTH, wtaRadio.IsChecked.Value);
                learned = false;

                for (int k = 0; k < dataVisual.Objects.Count; k++)
                {
                    for (int i = 0; i < dataVisual.Objects.Count; i++)
                    {
                        neural.Learn(new double[] { dataVisual.Objects[i].X, dataVisual.Objects[i].Y });
                    }
                    neural.StepUp();
                }
                learned = true;
            }
            if (Mode == ProcessMode.ProcessLearned || Mode == ProcessMode.LearnAndProcess)
            {
                if (learned)
                {
                    Dictionary<int, List<ClassObject>> cls = new Dictionary<int, List<ClassObject>>();
                    for (int i = 0; i < dataVisual.Objects.Count; i++)
                    {
                        int c = neural.Classify(new double[] { dataVisual.Objects[i].X, dataVisual.Objects[i].Y });
                        if (!cls.ContainsKey(c))
                        {
                            cls[c] = new List<ClassObject>();
        
                        }

                        cls[c].Add(dataVisual.Objects[i]);
                    }
                    mapVisual.Map = cls;
                    mapVisual.DrawContents();
                }
                else
                {
                    MessageBox.Show("Необходимо обучить нейронную сеть!");
                }
            }
        }

        private void classCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataVisual != null)
            {
                ComboBoxItem item = classCombo.SelectedItem as ComboBoxItem;
                if (item != null && item.Content != null)
                {
                    dataVisual.SelectedClass = Int32.Parse(item.Content.ToString());
                }
                else
                {
                    dataVisual.SelectedClass = 1;
                }
            }
        }

        private void neuronCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (neuronCountCombo.SelectedItem is ComboBoxItem &&
                (neuronCountCombo.SelectedItem as ComboBoxItem).Content != null)
            {
                int count = Int32.Parse((neuronCountCombo.SelectedItem as ComboBoxItem).Content.ToString());
                mapVisual.Dimension = count;
                mapVisual.DrawHexagons();
                neural = new NeuralNetwork(mapVisual.Dimension, INPUTS, dataVisual.Objects.Count, 
                    GRID_WIDTH, wtaRadio.IsChecked.Value);
                learned = false;
            }

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).Tag != null)
            {
                this.Mode = (ProcessMode)Int32.Parse((sender as RadioButton).Tag.ToString());
                learnTypeStack.IsEnabled = this.Mode != ProcessMode.ProcessLearned;

            }
            else
            {
                this.Mode = ProcessMode.Learn;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataVisual = new ObjectClassVisual();
            mapVisual = new MapVisual();
            dataVisual.SelectedClass = 1;
            customDrawPanel.Children.Add(dataVisual);
            somDrawPanel.Children.Add(mapVisual);
            mapVisual.Dimension = 2;
            mapVisual.DrawHexagons();
            mapVisual.Listener = dataVisual;
        }

    }
}
