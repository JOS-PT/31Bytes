using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private DrawingAttributes strokeAttr;

        public MainWindow()
        {
            InitializeComponent();
            // Pegamos a referência dos atributos padrão do InkCanvas definido no XAML
            strokeAttr = DrawingCanvas.DefaultDrawingAttributes;
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button && button.Content != null)
            {
                string modo = button.Content.ToString();

                if (modo == "Lápis")
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
                else if (modo == "Borracha")
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                else if (modo == "Selecionar")
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
            }
        }

        // Limpa todo o canvas (Botão Apagar inferior)
        private void EraseAll_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Strokes.Clear();
            ResultTextBlock.Text = "?";
        }

        // Executa a lógica de identificação (Botão Identificar)
        private void IdentifyNumber_Click(object sender, RoutedEventArgs e)
        {
            // Aqui entrará sua lógica MNIST futuramente.
            // Por enquanto, vamos simular um resultado.
            ResultTextBlock.Text = "7"; // Exemplo
        }

        private void DrawPanel_KeyUp(object sender, KeyEventArgs e)
        {
            int keyVal = (int)e.Key;
            if (keyVal >= 35 && keyVal <= 68) // Teclas 1, 2, 3, B, G
            {
                switch (keyVal)
                {
                    case 35: strokeAttr.Width = 2; strokeAttr.Height = 2; break; // Tecla 1
                    case 36: strokeAttr.Width = 4; strokeAttr.Height = 4; break; // Tecla 2
                    case 37: strokeAttr.Width = 6; strokeAttr.Height = 6; break; // Tecla 3
                    case 45: strokeAttr.Color = Colors.Blue; break;             // Tecla B
                    case 50: strokeAttr.Color = Colors.Green; break;            // Tecla G
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream fs = new FileStream("MyPicture.bin", FileMode.Create))
            {
                DrawingCanvas.Strokes.Save(fs);
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("MyPicture.bin"))
            {
                using (FileStream fs = new FileStream("MyPicture.bin", FileMode.Open, FileAccess.Read))
                {
                    DrawingCanvas.Strokes = new StrokeCollection(fs);
                }
            }
        }
    }
}