using Microsoft.ML.Data;

namespace Model
{
    public class SaidaModelo
    {
        [ColumnName("Plus214_Output_0")]
        [VectorType(1, 10)]
        public float[] Pontuacoes { get; set; } = new float[10];
    }
}
