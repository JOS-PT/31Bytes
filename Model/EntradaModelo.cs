using Microsoft.ML.Data;

namespace Model
{
    public class EntradaModelo
    {
        [ColumnName("Input3")]
        [VectorType(1, 1, 28, 28)]
        public float[] Pixels { get; set; } = new float[28 * 28];
    }
}
