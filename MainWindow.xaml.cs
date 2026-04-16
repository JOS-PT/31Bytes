using Microsoft.Win32;
using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32; // For SaveFileDialog


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


        private void ExportTo28x28(InkCanvas DrawingCanvas, string filePath)
        {
            int targetSize = 28;

            // 1. Capture the original size
            double originalWidth = DrawingCanvas.ActualWidth;
            double originalHeight = DrawingCanvas.ActualHeight;

            // 2. Create the 28x28 Bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                targetSize, targetSize, 96d, 96d, PixelFormats.Default);

            // 3. Create a scaling transform to shrink the canvas visual
            // This calculates the ratio needed to fit the canvas into 28px
            ScaleTransform scale = new ScaleTransform(
                targetSize / originalWidth,
                targetSize / originalHeight);

            // 4. Use a DrawingVisual to "draw" the scaled canvas
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.PushTransform(scale);
                // Draw the InkCanvas content onto this context
                dc.DrawRectangle(new VisualBrush(DrawingCanvas), null,
                    new Rect(new Point(0, 0), new Point(originalWidth, originalHeight)));
            }

            // 5. Render and Save
            rtb.Render(dv);




            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream fs = File.Open(filePath, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Criar a instância da janela de guardar ficheiro
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // 2. Configurar opções (filtros de extensão e nome padrão)
            saveFileDialog.Filter = "Bitmap Image (.bmp)|*.bmp|Binary file (.bin)|*.bin|Todos os ficheiros (*.*)|*.*";
            saveFileDialog.Title = "Escolha onde guardar o seu desenho";
            saveFileDialog.FileName = "MeuDesenho"; // Nome sugerido

            // 3. Mostrar a janela e verificar se o utilizador clicou em "Guardar"
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Criar um nome para o ficheiro binário baseado no caminho escolhido (opcional)
                // Aqui estou a guardar o binário com o mesmo nome mas extensão .bin
                string binPath = System.IO.Path.ChangeExtension(filePath, ".bin");

                try
                {
                    // Guardar a imagem 28x28 no caminho escolhido
                    ExportTo28x28(DrawingCanvas, filePath);

                    // Guardar os traços (strokes) para poder abrir mais tarde
                    using (FileStream fs = new FileStream(binPath, FileMode.Create))
                    {
                        DrawingCanvas.Strokes.Save(fs);
                    }

                    MessageBox.Show("Desenho guardado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao guardar: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Criar a caixa de diálogo para abrir
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Ficheiro de Traços (.bin)|*.bin";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        DrawingCanvas.Strokes = new StrokeCollection(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao abrir o ficheiro: " + ex.Message, "Erro");
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}