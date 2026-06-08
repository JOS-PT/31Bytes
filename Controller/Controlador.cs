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
            // O pedido de classificação é encaminhado para o Model, funcionando o
            // Controller assim como ligação entre a View e o Model.
            return classificador.ClassificarDesenho(bitmap);
        }
    }
}
