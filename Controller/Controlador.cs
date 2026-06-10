using System.Drawing;
using System.IO;
using Model;

namespace Controller
{
    public class Controlador
    {
        private const string NomeFicheiroModelo = "mnist-12.onnx";

        private readonly ClassificadorDesenho classificador;

        public Controlador()
        {
            // Deixei os detalhes de carregar o modelo para o Controlador, sendo este
            // que deve coordenar a aplicação.       
            string caminhoModelo = Path.Combine(AppContext.BaseDirectory,"Model",NomeFicheiroModelo);

            classificador = new ClassificadorDesenho(caminhoModelo);
        }

        public IResultadoClassificacao PedidoClassificacao(Bitmap bitmap)
        {
            return classificador.ClassificarDesenho(bitmap);
        }

        public List<IResultadoClassificacao> PedidoClassificacaoMultipla(Bitmap bitmap)
        {
            List<Bitmap> segmentos = PreprocessamentoImagem.SegmentarDigitos(bitmap);

            if (segmentos.Count == 0)
                throw new ArgumentException("O bitmap não contém píxeis desenhados.");

            var resultados = new List<IResultadoClassificacao>(segmentos.Count);
            foreach (Bitmap segmento in segmentos)
            {
                resultados.Add(classificador.ClassificarDesenho(segmento));
                segmento.Dispose();
            }

            return resultados;
        }
    }
}
