using Microsoft.ML;
using System.Drawing;

namespace Model
{
    public class ClassificadorDesenho
    {


        private const string NomeTensorEntrada = "Input3";
        private const string NomeTensorSaida = "Plus214_Output_0";
        private const int LarguraFinal = 28;
        private const int AlturaFinal = 28;
        private const int TotalPixels = LarguraFinal * AlturaFinal;

        private readonly PredictionEngine<EntradaModelo, SaidaModelo> motorPredicao;

        public ClassificadorDesenho(string caminhoModelo)
        {
            if (string.IsNullOrWhiteSpace(caminhoModelo))
            {
                throw new ArgumentException("O caminho do modelo não pode ser nulo ou vazio.", nameof(caminhoModelo));
            }

            if (!File.Exists(caminhoModelo))
            {
                throw new FileNotFoundException("Não foi encontrado nenhum modelo ONNX no caminho indicado.", caminhoModelo);
            }

            MLContext contextoML = new MLContext();
            IDataView dadosVazios = contextoML.Data.LoadFromEnumerable(Array.Empty<EntradaModelo>());

            var pipeline = contextoML.Transforms.ApplyOnnxModel(
                outputColumnNames: new[] { NomeTensorSaida },
                inputColumnNames: new[] { NomeTensorEntrada },
                modelFile: caminhoModelo
            );

            var modelo = pipeline.Fit(dadosVazios);
            motorPredicao = contextoML.Model.CreatePredictionEngine<EntradaModelo, SaidaModelo>(modelo);
        }

        private static float[] AplicarSoftmax(float[] pontuacoes)
        {
            if (pontuacoes == null)
            {
                throw new ArgumentException(nameof(pontuacoes));
            }

            if (pontuacoes.Length == 0)
            {
                throw new ArgumentException("O vetor de pontuções não pode estar vazio.", nameof(pontuacoes));
            }

            float maiorValor = pontuacoes[0];

            for (int i = 1; i < pontuacoes.Length; i++)
            {
                if (pontuacoes[i] > maiorValor)
                {
                    maiorValor = pontuacoes[i];
                }
            }

            double[] exponenciais = new double[pontuacoes.Length];
            double soma = 0.0;

            for (int i = 0; i < pontuacoes.Length; i++)
            {
                double valor = Math.Exp(pontuacoes[i] - maiorValor);
                exponenciais[i] = valor;
                soma += valor;
            }

            float[] probabilidades = new float[pontuacoes.Length];

            for (int i = 0; i < pontuacoes.Length; i++)
            {
                probabilidades[i] = (float)(exponenciais[i] / soma);
            }

            return probabilidades;
        }


        public ResultadoClassificacao ClassificarDesenho(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            using Bitmap imagemNormalizada = NormalizarImagem(bitmap);
            using Bitmap imagemRecortada = PreprocessamentoImagem.RecortarAreaUtil(imagemNormalizada);
            using Bitmap imagemPreparada = PreprocessamentoImagem.AjustarParaCanvas28x28(imagemRecortada);
            float[] pixels = ConverterParaFloatArray(imagemPreparada);

            return ExecutarPredicaoONNX(pixels);
        }

        private static Bitmap NormalizarImagem(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            Bitmap imagemNormalizada = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color corOriginal = bitmap.GetPixel(x, y);
                    int cinzento = (int)Math.Round(0.299 * corOriginal.R + 0.587 * corOriginal.G + 0.114 * corOriginal.B);

                    Color corNormalizada = Color.FromArgb(cinzento, cinzento, cinzento);
                    imagemNormalizada.SetPixel(x, y, corNormalizada);
                }
            }

            return imagemNormalizada;
        }

        private static Bitmap RedimensionarPara28x28(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            return new Bitmap(bitmap, new Size(LarguraFinal, AlturaFinal));
        }

        private static float[] ConverterParaFloatArray(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            float[] pixels = new float[TotalPixels];

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    float cinzento = pixel.R / 255f;
                    float valorNormalizado = 1f - cinzento;

                    int indice = y * LarguraFinal + x;
                    pixels[indice] = valorNormalizado;
                }
            }

            return pixels;
        }

        private ResultadoClassificacao ExecutarPredicaoONNX(float[] pixels)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (pixels.Length != TotalPixels)
            {
                throw new ArgumentException(
                    $"A imagem tem de conter exatamente {TotalPixels} valores.",
                    nameof(pixels)
                );
            }

            var entrada = new EntradaModelo { Pixels = pixels };
            SaidaModelo saida = motorPredicao.Predict(entrada);

            if (saida == null)
            {
                throw new InvalidOperationException("O modelo devolveu uma predição nula.");
            }

            if (saida.Pontuacoes == null || saida.Pontuacoes.Length != 10)
            {
                throw new InvalidOperationException("O modelo devolveu um vector de saída inválido.");
            }

            float[] probabilidades = AplicarSoftmax(saida.Pontuacoes);

            int melhorIndice = 0;
            float maiorProbabilidade = probabilidades[0];

            for (int i = 1; i < probabilidades.Length; i++)
            {
                if (probabilidades[i] > maiorProbabilidade)
                {
                    maiorProbabilidade = probabilidades[i];
                    melhorIndice = i;
                }
            }

            return new ResultadoClassificacao
            {
                Digito = melhorIndice,
                Confianca = maiorProbabilidade
            };
        }
    }
}
