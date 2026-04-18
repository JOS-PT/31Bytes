# Explicação

Exemplo de utilização:

```csharp
using Model;
using System.Drawing;

string caminhoModelo = "mnist-12.onnx";

ClassificadorDesenho classificador = new ClassificadorDesenho(caminhoModelo);

using Bitmap bitmap = new Bitmap("bitmap.bmp");

ResultadoClassificacao resultado = classificador.ClassificarDesenho(bitmap);

Console.WriteLine($"Digito previsto: {resultado.Digito}");
Console.WriteLine($"Confianca: {resultado.Confianca:P1}");
```

O método `ClassificarDesenho(Bitmap bitmap)` executa o seguinte processo: normaliza a imagem para *grayscale*; recorta a área útil do desenho; ajusta a imagem para *canvas* `28x28` (Se a imagem já vier nesta forma, então o melhor é remover esta parte);converte a imagem para `float[]`; calcula a predição ONNX e devolve o resultado final

O mtodo devolve um objeto do tipo `ResultadoClassificacao`. Cuja estrutura é:

```csharp
public class ResultadoClassificacao
{
    public int Digito { get; set; }
    public float Confianca { get; set; }
}
```

`Digito` é um digito previsto pelo modelo,`Confianca` é o valor da confiança da previsão e pode ser mostrado como percentagem com `:P1` (O `mnist-12` devolve um valor que não é uma percentagem, daí a utilização do método `AplicarSoftmax()` para converter o vetor para percentagens)

Para o método funcionar necessita de um `Bitmap` válido com o desenho. O modelo é muito sensível ao formato dos bitmaps, que têm de estar mesmo muito próximos dos bitmaps usados no treino do modelo. Qualquer pequena rotação do número afeta a previsão (4's para 9's por exemplo). E o ficheiro `mnist-12.onnx` acessível. Nos testes que fiz, pus o ficheiro `mnist-12.onnx` ao mesmo nível que `Program.cs` e adicionei isto a `.csproj`:

```xml
  <ItemGroup>
	 <None Update="mnist-12.onnx">
	 <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
	 </None>
  </ItemGroup>
```

Para usar os `TestBitmaps` é também necessário adicionar:

```xml
  <ItemGroup>
	 <None Update="TestBitmaps\**\*.*">
	 <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
	 </None>
  </ItemGroup>
```

O primeiro bloco faz com que o ficheiro `mnist-12.onnx` seja incluído na pasta de output da compilação. Assim, quando a aplicação corre, o modelo fica disponível ao lado do executável e pode ser carregado sem ser preciso copiar o ficheiro manualmente.

O segundo bloco faz o mesmo para todos os ficheiros dentro de `TestBitmaps` e das respetivas subpastas. 

Para usar este código o projeto deve ter estes *nuget packages* instalados:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.ML" Version="5.0.0" />
  <PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.24.4" />
  <PackageReference Include="Microsoft.ML.OnnxTransformer" Version="5.0.0" />
  <PackageReference Include="System.Drawing.Common" Version="10.0.5" />
</ItemGroup>
```

