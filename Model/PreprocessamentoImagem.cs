using System.Drawing;

namespace Model
{
    public static class PreprocessamentoImagem
    {
        private const int LarguraFinal = 28;
        private const int AlturaFinal = 28;
        private const int CaixaInterna = 20;
        private const int LimiteTraco = 245;

        private static (double cx, double cy) CalcularCentroDeMassa(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            double somaPesos = 0.0;
            double somaX = 0.0;
            double somaY = 0.0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    double peso = 1.0 - (pixel.R / 255.0);

                    if (peso <= 0.0)
                    {
                        continue;
                    }

                    somaPesos += peso;
                    somaX += x * peso;
                    somaY += y * peso;
                }
            }

            if (somaPesos <= 0.0)
            {
                return (bitmap.Width / 2.0, bitmap.Height / 2.0);
            }

            return (somaX / somaPesos, somaY / somaPesos);
        }

        public static Bitmap RecortarAreaUtil(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            int minX = bitmap.Width;
            int minY = bitmap.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);

                    if (pixel.R < LimiteTraco)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return new Bitmap(bitmap);
            }

            int largura = maxX - minX + 1;
            int altura = maxY - minY + 1;

            Bitmap recorte = new Bitmap(largura, altura);

            using Graphics grafico = Graphics.FromImage(recorte);
            grafico.DrawImage(
                bitmap,
                new Rectangle(0, 0, largura, altura),
                new Rectangle(minX, minY, largura, altura),
                GraphicsUnit.Pixel
            );

            return recorte;
        }

        public static Bitmap AjustarParaCanvas28x28(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            double escala = Math.Min(
                (double)CaixaInterna / bitmap.Width,
                (double)CaixaInterna / bitmap.Height
            );

            int novaLargura = Math.Max(1, (int)Math.Round(bitmap.Width * escala));
            int novaAltura = Math.Max(1, (int)Math.Round(bitmap.Height * escala));

            using Bitmap redimensionado = new Bitmap(novaLargura, novaAltura);

            using (Graphics graficoResize = Graphics.FromImage(redimensionado))
            {
                graficoResize.Clear(Color.White);
                graficoResize.DrawImage(bitmap, 0, 0, novaLargura, novaAltura);
            }

            (double cx, double cy) = CalcularCentroDeMassa(redimensionado);

            int offsetX = (int)Math.Round((LarguraFinal / 2.0) - cx);
            int offsetY = (int)Math.Round((AlturaFinal / 2.0) - cy);

            offsetX = Math.Clamp(offsetX, 0, LarguraFinal - novaLargura);
            offsetY = Math.Clamp(offsetY, 0, AlturaFinal - novaAltura);

            Bitmap canvas = new Bitmap(LarguraFinal, AlturaFinal);

            using (Graphics grafico = Graphics.FromImage(canvas))
            {
                grafico.Clear(Color.White);
                grafico.DrawImage(redimensionado, offsetX, offsetY);
            }

            return canvas;
        }
    }
}
